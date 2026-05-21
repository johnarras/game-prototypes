using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Linq;

using System.Threading;
using UnityEngine;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Logalytics.Interfaces;
using System.Threading.Tasks;
using OxDb.SharedGame.DataStores.Utils;
using Assets.Scripts.Awaitables;
using Assets.Scripts.Assets;
using System.Collections.Concurrent;
using Assets.Scripts.Assets.Entities;
using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Assets.Bundles;
using Assets.Scripts.Assets.Services;
using OxDb.SharedCore.Interfaces;

using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Core;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Serialization.Interfaces;
using Assets.Scripts.Repository;
using OxDb.SharedCore.DataStores.Entities;












#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IAssetService : IInitializable, IClientResetCleanup
{
    bool IsInitialized();
    void LoadAssetInto<T>(object parent, string assetCategory, string assetPath, AssetDownloadHandler<T> handler, CancellationToken token, T data = default(T), string subdirectory = null);
    void LoadAsset<T>(string assetCategory, string assetPath, AssetDownloadHandler<T> handler, object parent, CancellationToken token, T data = default(T), string subdirectory = null);
    Task<T> LoadAssetAsync<T>(string assetCategory, string assetPath, object parent, CancellationToken token, string subdirectory = null) where T : class;
    Task<object> LoadAssetAsync(string assetCategory, string assetPath, object parent, CancellationToken token, string subdirectory = null);
    string GetBundleNameForCategoryAndAsset(string assetCategory, string assetPath);
    ClientAssetCounts GetAssetCounts();
    string StripPathPrefix(string path);
    void SetWorldAssetEnv(string worldAssetEnv);
    string GetContentRootURL(EDataCategories category);
    bool IsDownloading();
    string GetWorldDataEnv();
    void UnloadAsset(object obj);
    Awaitable UnloadUnusedAssetsAsync();
    string GetAssetPath(string assetCategoryName);
    void SetLoadSpeed(ELoadSpeed speed);

}
public class UnityAssetService : IAssetService, IAssetSubsystem
{
    private ILogService _logService = null;
    private IFileDownloadService _fileDownloadService = null;
    protected IClientRandom _rand = null;
    protected IClientGameState _gs = null;
    protected IClientEntityService _clientEntityService = null;
    private IClientConfigContainer _config = null;
    private IClientAppService _clientAppService = null;
    private ITextSerializer _serializer = null;
    private IClientRepositoryService _clientRepositoryService = null;

    private IAwaitableService _awaitableService = null;
    private ILocalLoadService _localLoadService = null;


    private const int _maxConcurrentDownloads = 8;

    private const int _retryTimes = 3;

    protected bool _isInitialized = false;

    private Dictionary<EDataCategories, string> _urlPrefixes = new Dictionary<EDataCategories, string>();
    private Dictionary<EDataCategories, string> _assetEnvs = new Dictionary<EDataCategories, string>();

    protected HashSet<string> _bundleFailedDownloads = new HashSet<string>();

    protected HashSet<string> _failedLocalLoads = new HashSet<string>();
    private Dictionary<string, GameObject> _localLoads = new Dictionary<string, GameObject>();

    protected Dictionary<string, SpriteAtlasContainer> _atlasCache = new Dictionary<string, SpriteAtlasContainer>();

    protected BundleVersions _bundleVersions = null;
    protected BundleUpdateInfo _bundleUpdateInfo = null;

    private ConcurrentQueue<IBundleDownload>[] _downloadQueues = null;
    protected Dictionary<string, BundleCacheData> _bundleCache = new Dictionary<string, BundleCacheData>();

    private string _contentRootUrl = null;

    private CancellationToken _token = CancellationToken.None;

    private ClientAssetCounts _assetCounts = new ClientAssetCounts();

    public ClientAssetCounts GetAssetCounts()
    {
        return _assetCounts;
    }

    public string GetWorldDataEnv()
    {
        return _assetEnvs[EDataCategories.Worlds];
    }

    private BundleVersion GetBundleVersion(string bundleName)
    {
        if (_bundleVersions.Versions.TryGetValue(bundleName, out BundleVersion version))
        {
            return version;
        }
        return null;
    }

    private BundleCacheData GetBundleCacheData(string bundleName)
    {
        if (_bundleCache.TryGetValue(bundleName, out BundleCacheData bundleCacheData))
        {
            return bundleCacheData;
        }
        return null;
    }

    private List<IAssetSubsystem> _assetSubsystems = null;
    private int _assetSubsystemIndex = 0;
    private async Awaitable UpdateAssetSubsystems(CancellationToken token)
    {
        if (_assetSubsystems == null)
        {
            _assetSubsystems = _gs.loc.GetVals<IAssetSubsystem>();
        }

        if (_assetSubsystems.Count < 1)
        {
            return;
        }

        while (true)
        {
            IAssetSubsystem subsystem = _assetSubsystems[_assetSubsystemIndex++ % _assetSubsystems.Count];

            await subsystem.UpdateAssets(token);

            await Awaitable.WaitForSecondsAsync(0.35f);
        }
    }

    public async Task Initialize(CancellationToken token)
    {
        if (!_clientAppService.IsPlaying)
        {
            return;
        }

        _token = token;
        _contentRootUrl = _config.Config.ContentEndpoint;
        SetAssetEnv(EDataCategories.Assets, _config.Config.AssetsEnv);
        SetAssetEnv(EDataCategories.Worlds, _config.Config.WorldsEnv);

        string persPath = _clientAppService.PersistentDataPath;
        _downloadQueues = new ConcurrentQueue<IBundleDownload>[_maxConcurrentDownloads];

        for (int i = 0; i < _maxConcurrentDownloads; i++)
        {
            _downloadQueues[i] = new ConcurrentQueue<IBundleDownload>();
            _awaitableService.ForgetAwaitable(ProcessBundleQueue(_downloadQueues[i]));
        }

        _awaitableService.ForgetAwaitable(UpdateAssetSubsystems(_token));

        if (_config.Config.SelfContainedClient)
        {
            LoadLocalBundleInit();
        }
        else
        {
            LoadLastSaveTimeFile(token);
        }

        SetLoadSpeed(ELoadSpeed.Normal);
        await Task.CompletedTask;
    }

    public void SetLoadSpeed(ELoadSpeed speed)
    {
        if (speed == ELoadSpeed.Normal)
        {
            Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.Normal;
        }
        else
        {
            Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.High;
        }
    }


    private void LoadLocalBundleInit()
    {
        _bundleUpdateInfo = new BundleUpdateInfo() { UpdateTime = DateTime.UtcNow.Date };
        // Only bail out if we are in editor and not testing local loads.
#if UNITY_EDITOR
        if (!InitClient.EditorInstance.TestLocalBundles)
        {
            _bundleVersions = new BundleVersions() { UpdateInfo = _bundleUpdateInfo };
            _isInitialized = true;
            return;
        }
#endif

        TextAsset textAsset = _localLoadService.LocalLoad<TextAsset>("Config/" + AssetConstants.BundleVersionsFile.Replace(".txt", ""));


        if (textAsset != null && !string.IsNullOrEmpty(textAsset.text))
        {
            _bundleVersions = SetupBundleVersions(_serializer.Deserialize<BundleVersions>(textAsset.text));
        }

        if (_bundleVersions == null)
        {

            _bundleVersions = new BundleVersions() { UpdateInfo = _bundleUpdateInfo };
        }

        _isInitialized = true;
    }

    public bool IsDownloading()
    {

        if (_downloadQueues.FastAny(x => !x.IsEmpty))
        {
            return true;
        }

        return false;
    }

    public async Task OnReset(CancellationToken token)
    {
        await ClearBundleCache();
    }

    public async Task ClearBundleCache()
    {
        Dictionary<string, BundleCacheData> newBundleCache = new Dictionary<string, BundleCacheData>();

        foreach (string item in _bundleCache.Keys)
        {
            BundleCacheData bdata = _bundleCache[item];

            if (bdata.assetBundle != null)
            {
                FullyUnloadAssetBundle(bdata);
            }
        }

        _bundleCache = newBundleCache;
        await UnloadUnusedAssetsAsync();
    }

    private void FullyUnloadAssetBundle(BundleCacheData bundleCache)
    {
        _logService.Info("DeleteBundle: " + bundleCache.Name + " -- " + _bundleCache.Keys.Count);
        bundleCache.assetBundle.Unload(true);
        _assetCounts.BundlesUnloaded++;
        _clientEntityService.Destroy(bundleCache.assetBundle);

        foreach (object obj in bundleCache.LoadedAssets)
        {
            _clientEntityService.Destroy(obj);
        }
        bundleCache.LoadedAssets = new Dictionary<string, GameObject>();

        foreach (BundleCacheData childBundle in bundleCache.ChildDependencies)
        {
            childBundle.ParentDependencies.Remove(bundleCache);
        }

        bundleCache.ChildDependencies.Clear();

    }

    public async Awaitable UpdateAssets(CancellationToken token)
    {
        int removeCount = 0;
        List<string> bundleCacheKeys = _bundleCache.Keys.ToList();
        foreach (string item in bundleCacheKeys)
        {
            if (_bundleCache.TryGetValue(item, out BundleCacheData bundle))
            {
                if (bundle.LoadingCount < 1 &&
                    bundle.assetBundle != null &&
                    bundle.LastUsed < DateTime.UtcNow.AddSeconds(-3) &&
                    bundle.ParentDependencies.Count < 1)
                {
                    if (bundle.DeleteTicks > 0)
                    {
                        bundle.DeleteTicks--;
                        continue;
                    }

                    if (bundle.Instances.FastAny(x => x.Equals(null)))
                    {
                        bundle.Instances = bundle.Instances.Where(x => !x.Equals(null)).ToList();
                    }
                    if (bundle.Instances.Count > 0)
                    {
                        continue;
                    }
                    bundle.LastUsed = DateTime.UtcNow;
                    _bundleCache.Remove(item);

                    FullyUnloadAssetBundle(bundle);

                    removeCount++;
                    if (removeCount > 5)
                    {
                        break;
                    }
                    await Awaitable.NextFrameAsync();
                    if (!TokenUtils.IsValid(_token))
                    {
                        return;
                    }
                }
                else
                {
                    bundle.DeleteTicks = 2;
                }
            }
        }
        if (removeCount > 0)
        {
            await UnloadUnusedAssetsAsync();
        }
    }

    private bool _unloadingAssets = false;
    public async Awaitable UnloadUnusedAssetsAsync()
    {
        if (_unloadingAssets)
        {
            return;
        }
        _unloadingAssets = true;
        _localLoads.Clear();
        AsyncOperation op = Resources.UnloadUnusedAssets();
        while (!op.isDone)
        {
            await Awaitable.NextFrameAsync();
            if (!TokenUtils.IsValid(_token))
            {
                return;
            }
        }
        _unloadingAssets = false;
    }

    private string GetAssetNameFromPath(AssetBundle assetBundle, string assetName)
    {
        if (assetBundle == null || string.IsNullOrEmpty(assetName))
        {
            return assetName;
        }

        assetName = assetName.ToLower();
        if (assetName.IndexOf("/") >= 0)
        {
            assetName = assetName.Substring(assetName.LastIndexOf("/") + 1);
        }

        string fullAssetName = assetName;
        string[] assetNames = assetBundle.GetAllAssetNames();
        for (int i = 0; i < assetNames.Length; i++)
        {
            if (assetNames[i].IndexOf(assetName) >= 0)
            {
                if (assetNames[i].LastIndexOf("/") == assetNames[i].LastIndexOf(assetName) - 1)
                {
                    fullAssetName = assetNames[i];
                    break;
                }
            }
        }
        return fullAssetName;
    }

    public string GetAssetPath(string assetCategoryName)
    {
        return assetCategoryName + "/";
    }

    /// <summary>
    /// Download something from an asset bundle (Async)
    /// </summary>
    /// <param name="gs"></param>
    /// <param name="assetPathSuffix">This is the category where the asset resides. It exists here so that
    /// the URLs being stored by the game aren't as long and so that we can move the categories on disk
    /// in a single spot (enforced here rather than making sure all data items use it.) It's a tradeoff
    /// and I've gone back and forth, but if it isn't here, then it would have to be in each
    /// piece of data stored in game data, OR each time the asset is loaded, we would have to do
    /// a lookup to get the path from the category OR it would have to be hardcoded. So this seems
    /// like the best way to do it to avoid mistakes later on even though it costs a bit extra in
    /// terms of programming time to put the category in the load. </param>
    /// <param name="assetName"></param>
    /// <param name="handler"></param>
    /// <param name="data"></param>
    /// <param name="assetPathSuffix">optional category used for certain specific naming conventions for bundles</param>
    public void LoadAsset<T>(string assetPathSuffix, string assetName,
            AssetDownloadHandler<T> handler,
             object parentIn,
            CancellationToken token, T data = default(T), string subdirectory = null)
    {
        GameObject parent = parentIn as GameObject;

        if (parent == null)
        {
            MonoBehaviour mb = parentIn as MonoBehaviour;
            if (mb != null)
            {
                parent = mb.gameObject;
            }
        }

        if (string.IsNullOrEmpty(assetName))
        {
            return;
        }

        if (!string.IsNullOrEmpty(subdirectory))
        {
            assetPathSuffix += "/" + subdirectory;
        }
        if (_config.Config.SelfContainedClient)
        {
            string categoryPath = GetAssetPath(assetPathSuffix);
            string fullAssetName = categoryPath + assetName;
            if (!String.IsNullOrEmpty(categoryPath) &&
            !_failedLocalLoads.Contains(fullAssetName))
            {
#if UNITY_EDITOR

                GameObject asset = null;
                string fullPath = "";
                if (!InitClient.EditorInstance.TestLocalBundles)
                {

                    fullPath = AssetConstants.DownloadAssetRootPath + fullAssetName +
                    (assetName.IndexOf(AssetConstants.ArtFileSuffix) < 0 ? AssetConstants.ArtFileSuffix : "");
                    if (_localLoads.ContainsKey(fullPath))
                    {
                        asset = _localLoads[fullPath];
                    }
                    else
                    {
                        asset = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
                        if (asset is GameObject go)
                        {
                            _localLoads[fullPath] = go;
                        }
                    }

                    if (asset == null && assetName.IndexOf("_") != 0)
                    {
                        fullPath = fullPath.Replace(AssetConstants.DownloadAssetRootPath, AssetConstants.DownloadAssetRootPath + "_");
                        asset = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
                    }

                    if (asset != null)
                    {
                        asset = InstantiateIntoParent(asset, parent);

                        _clientEntityService.InitializeHierarchy(asset);

                        if (handler != null)
                        {
                            handler(asset, data, token);
                        }
                    }
                    else
                    {
                        _failedLocalLoads.Add(assetName);

                        if (handler != null)
                        {
                            handler(asset, data, token);
                        }
                    }
                    return;
                }
#endif
            }
        }

        string bundleName = GetBundleNameForCategoryAndAsset(assetPathSuffix, assetName);

        if (_bundleFailedDownloads.Contains(bundleName))
        {
            return;
        }

        if (assetName.LastIndexOf("/") >= 0)
        {
            assetName = assetName.Substring(assetName.LastIndexOf("/") + 1);
        }

        if (_bundleCache.TryGetValue(bundleName, out BundleCacheData cacheData))
        {
            if (cacheData.LoadedAssets.TryGetValue(assetName, out GameObject loadedAsset))
            {
                BundleDownload<T> download = new BundleDownload<T>()
                {
                    parent = parent,
                    handler = handler,
                    data = data,
                    Token = token,
                };

                LoadCachedAsset(cacheData, loadedAsset, download);
                return;
            }
        }

        AddBundleDownload(bundleName, assetName, handler, data, parent, new List<string>(), token);
    }

    private void QueueBundleDependencies<T>(string bundleName, List<string> parentBundles, CancellationToken token)
    {
        if (parentBundles.Contains(bundleName))
        {
            return;
        }

        if (_bundleVersions.Versions.TryGetValue(bundleName, out BundleVersion version))
        {
            if (version.ChildDependencies.Count < 1)
            {
                return;
            }

            foreach (string dependency in version.ChildDependencies)
            {
                parentBundles.Add(dependency);
                AddBundleDownload<T>(dependency, null, null, default(T), null, parentBundles, token);
            }
        }
    }

    private void AddBundleDownload<T>(string bundleName, string assetName, AssetDownloadHandler<T> handler, T data, GameObject parent, List<string> parentBundles, CancellationToken token)
    {

        BundleDownload<T> bundleDownload = null;
        bundleDownload = new BundleDownload<T>();
        bundleDownload.bundleName = bundleName;
        bundleDownload.assetName = assetName;
        bundleDownload.handler = handler;
        bundleDownload.data = data;
        bundleDownload.parent = parent;
        bundleDownload.Token = token;
        bundleDownload.url = GetFullBundleURL(bundleName);
        bundleDownload.idHash = StrUtils.GetSimpleFullStringHash(bundleName);

        if (_bundleVersions.Versions.TryGetValue(bundleName, out BundleVersion version))
        {
            bundleDownload.isLocal = version.IsLocal;
        }

        if (_bundleCache.ContainsKey(bundleName))
        {
            _bundleCache[bundleName].LastUsed = DateTime.UtcNow;
        }

        QueueBundleDependencies<T>(bundleName, parentBundles, token);

        _downloadQueues[bundleDownload.idHash % _downloadQueues.Length].Enqueue(bundleDownload);

    }

    public void SetWorldAssetEnv(string worldAssetEnv)
    {
        SetAssetEnv(EDataCategories.Worlds, worldAssetEnv);
    }

    private void SetAssetEnv(EDataCategories category, string env)
    {
        string containerName = BlobUtils.GetBlobContainerName(category.ToString(), _config.Config.GameMode.ToString(), env);

        _urlPrefixes[category] = _contentRootUrl + "/" + containerName + "/";
        _assetEnvs[category] = env;
    }

    public string GetContentRootURL(EDataCategories dataCategory)
    {
        if (_urlPrefixes.TryGetValue(dataCategory, out string url))
        {
            return url;
        }

        return null;
    }

    public bool IsInitialized()
    {
        return _isInitialized;
    }

    private async Awaitable ProcessBundleQueue(ConcurrentQueue<IBundleDownload> queue)
    {
        await Awaitable.NextFrameAsync();
        if (!TokenUtils.IsValid(_token))
        {
            return;
        }

        List<IBundleDownload> requeueList = new List<IBundleDownload>();
        while (true)
        {
            while (queue.TryDequeue(out IBundleDownload download))
            {
                if (!_bundleCache.ContainsKey(download.bundleName))
                {
                    if (HaveAllDependencies(download))
                    {
                        await DownloadOneBundle(download);
                        if (!TokenUtils.IsValid(_token))
                        {
                            return;
                        }
                    }
                    else
                    {
                        requeueList.Add(download);
                        continue;
                    }
                }

                if (_bundleCache.TryGetValue(download.bundleName, out BundleCacheData cacheData))
                {
                    await LoadAssetFromExistingBundle(download, cacheData);
                }
            }
            foreach (IBundleDownload download in requeueList)
            {
                queue.Enqueue(download);
            }
            requeueList.Clear();
            await Awaitable.NextFrameAsync();
            if (!TokenUtils.IsValid(_token))
            {
                return;
            }

        }
    }

    private bool HaveAllDependencies(IBundleDownload download)
    {
        BundleVersion version = GetBundleVersion(download.bundleName);

        if (version == null || version.ChildDependencies == null || version.ChildDependencies.Count == 0)
        {
            return true;
        }
        foreach (string dep in version.ChildDependencies)
        {
            BundleCacheData cacheData = GetBundleCacheData(dep);
            if (cacheData != null)
            {
                cacheData.LastUsed = DateTime.UtcNow;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    private DateTime _lastAssetLoadTime = DateTime.UtcNow;
    private async Awaitable LoadAssetFromExistingBundle(IBundleDownload bdl, BundleCacheData cacheData)
    {
        // Need to check existence of bundle here since this call is delayed from when 
        if (bdl == null || string.IsNullOrEmpty(bdl.assetName))
        {
            return;
        }

        cacheData.LoadingCount++;
        cacheData.LastUsed = DateTime.UtcNow;
        if (!cacheData.LoadedAssets.ContainsKey(bdl.assetName))
        {
            AssetBundleRequest request = StartLoadAssetFromBundle(bdl.bundleName, bdl.assetName);
            if (request != null)
            {
                while (!request.isDone)
                {
                    await Awaitable.NextFrameAsync();
                    if (!TokenUtils.IsValid(_token))
                    {
                        return;
                    }
                }
                _lastAssetLoadTime = DateTime.UtcNow;
                cacheData.LoadedAssets[bdl.assetName] = request.asset as GameObject;
            }
        }

        if (cacheData.LoadedAssets.TryGetValue(bdl.assetName, out GameObject obj))
        {
            cacheData.LoadingCount--;

            LoadCachedAsset(cacheData, obj, bdl);
        }
        else
        {
            cacheData.LoadingCount--;
            return;
        }
    }

    private void LoadCachedAsset(BundleCacheData cacheData, GameObject cachedObject, IBundleDownload download)
    {
        GameObject newObj = InstantiateBundledAsset(cacheData, cachedObject, download.parent);
        download.CallDownloadHandler(newObj, _logService);
    }

    private void AddBundleToCache(IBundleDownload bad, AssetBundle downloadedBundle)
    {
        if (downloadedBundle == null || _bundleCache.ContainsKey(bad.bundleName))
        {
            return;
        }

        BundleCacheData bdata = new BundleCacheData()
        {
            Name = bad.bundleName,
            assetBundle = downloadedBundle,
            LastUsed = DateTime.UtcNow,
        };

        _bundleCache[bad.bundleName] = bdata;
        _assetCounts.BundlesLoaded++;

        BundleVersion version = GetBundleVersion(bad.bundleName);

        if (version != null)
        {
            foreach (string dep in version.ChildDependencies)
            {

                BundleCacheData childData = GetBundleCacheData(dep);

                if (childData == null)
                {
                    _logService.Error("Bundle " + bad.bundleName + " is missing dependency bundle " + dep);
                    continue;
                }
                childData.ParentDependencies.Add(bdata);
                bdata.ChildDependencies.Add(childData);
            }
        }
    }

    private AssetBundleRequest StartLoadAssetFromBundle(string bundleName, string assetName)
    {
        if (string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(assetName))
        {
            return null;
        }
        string fullname = (bundleName + "--" + assetName).ToLower();

        if (!_bundleCache.ContainsKey(bundleName))
        {
            return null;
        }

        BundleCacheData cacheData = _bundleCache[bundleName];
        AssetBundle bundle = cacheData.assetBundle;
        string fullName = GetAssetNameFromPath(bundle, assetName);

        try
        {
            return bundle.LoadAssetAsync(assetName);
        }
        catch (Exception e)
        {
            _logService.Exception(e, "Failed asset Load:" + assetName);
        }
        return null;
    }


    protected string GetBundleHash(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName)) return "";
        if (!_bundleVersions.Versions.ContainsKey(bundleName))
        {
            return "";
        }
        return _bundleVersions.Versions[bundleName].Hash;
    }

    protected uint[] GetBundleHashInts(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName)) return null;
        if (!_bundleVersions.Versions.ContainsKey(bundleName))
        {
            return null;
        }
        return _bundleVersions.Versions[bundleName].GetHashInts();
    }

    protected Hash128 GetBundleHash128(string bundleName)
    {
        uint[] hashInts = GetBundleHashInts(bundleName);
        if (hashInts == null || hashInts.Length != 4) return new Hash128();
        return new Hash128(hashInts[0], hashInts[1], hashInts[2], hashInts[3]);
    }


    protected void LoadLastSaveTimeFile(CancellationToken token)
    {
        string path = _clientAppService.GetRuntimePrefix() + AssetConstants.BundleUpdateTimeFile;
        DownloadFileData ddata = new DownloadFileData()
        {
            ForceDownload = true,
            Handler = OnDownloadLastSaveTimeText,
            IsText = true,
            Category = EDataCategories.Assets,
        };
        _fileDownloadService.DownloadFile(path, ddata, token);
    }

    private void OnDownloadLastSaveTimeText(object obj, object data, CancellationToken token)
    {
        if (obj is string str)
        {
            _bundleUpdateInfo = _serializer.Deserialize<BundleUpdateInfo>(str);
        }

        if (_bundleUpdateInfo == null)
        {
            _bundleUpdateInfo = new BundleUpdateInfo() { UpdateTime = DateTime.UtcNow.Date };
        }

        LoadAssetBundleList(token);
    }

    private BundleVersions SetupBundleVersions(BundleVersions versions)
    {
        if (versions == null)
        {
            return versions;
        }

        foreach (BundleVersion version in versions.Versions.Values)
        {
            foreach (string dep in version.ChildDependencies)
            {
                if (versions.Versions.TryGetValue(dep, out BundleVersion childBundle))
                {
                    if (!childBundle.ParentDependencies.Contains(dep))
                    {
                        childBundle.ParentDependencies.Add(dep);
                    }
                }
            }
        }

        return versions;
    }

    void LoadAssetBundleList(CancellationToken token)
    {
        _bundleVersions = SetupBundleVersions(_clientRepositoryService.Load<BundleVersions>(AssetConstants.BundleVersionsFile).Result);

        if (_bundleVersions == null || _bundleVersions.UpdateInfo == null ||
            _bundleVersions.UpdateInfo.ClientVersion != _bundleUpdateInfo.ClientVersion ||
            _bundleVersions.UpdateInfo.UpdateTime != _bundleUpdateInfo.UpdateTime)
        {
            DownloadFileData ddata = new DownloadFileData()
            {
                ForceDownload = true,
                Handler = OnDownloadBundleVersions,
                IsText = true,
                Category = EDataCategories.Assets,
            };

            string path = _clientAppService.GetRuntimePrefix() + AssetConstants.BundleVersionsFile;
            path += ("?timestamp=" + _bundleUpdateInfo.UpdateTime.Ticks);
            _fileDownloadService.DownloadFile(path, ddata, token);
            _logService.Info("YES DOWNLOAD BUNDLE VERSIONS!");
        }
        else
        {
            _isInitialized = true;
            _logService.Info("NO DOWNLOAD BUNDLE VERSIONS");
        }
    }

    private void OnDownloadBundleVersions(object obj, object data, CancellationToken token)
    {
        BundleVersions newVersions = null;

        if (obj is string str)
        {
            newVersions = _serializer.Deserialize<BundleVersions>(str);
        }
        _isInitialized = true;

        if (newVersions != null && newVersions.UpdateInfo != null &&
            newVersions.Versions != null && newVersions.Versions.Keys.Count > 0)
        {
            _bundleVersions = SetupBundleVersions(newVersions);
            RepoSaveArgs repoArgs = new RepoSaveArgs()
            {
                OverrideId = AssetConstants.BundleVersionsFile
            };
            _clientRepositoryService.Save(_bundleVersions, repoArgs);
        }
    }

    protected string GetFullBundleURL(string bundleName)
    {
        return GetContentRootURL(EDataCategories.Assets) + _clientAppService.GetRuntimePrefix() + bundleName + "_" + GetBundleHash(bundleName);
    }

    private async Awaitable DownloadOneBundle(IBundleDownload bad)
    {
        if (string.IsNullOrEmpty(bad.url) || !TokenUtils.IsValid(bad.Token))
        {
            return;
        }

        BundleVersion version = GetBundleVersion(bad.bundleName);

        if (version != null)
        {
            foreach (string dep in version.ChildDependencies)
            {

                BundleCacheData childData = null;

                while (childData == null)
                {
                    childData = GetBundleCacheData(dep);

                    if (childData == null)
                    {
                        await Awaitable.NextFrameAsync();
                        if (!TokenUtils.IsValid(_token) || !TokenUtils.IsValid(bad.Token))
                        {
                            return;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < _retryTimes; i++)
        {

            if (!_config.Config.SelfContainedClient && !bad.isLocal)
            {
                string bundleHash = GetBundleHash(bad.bundleName);
                if (string.IsNullOrEmpty(bundleHash))
                {
                    _logService.Debug("No bundle hash for: " + bad.url);
                    return;
                }

                using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(bad.url,
                    GetBundleHash128(bad.bundleName)))
                {
                    UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
                    while (!asyncOp.isDone)
                    {
                        await Awaitable.NextFrameAsync();
                        if (!TokenUtils.IsValid(_token) || !TokenUtils.IsValid(bad.Token))
                        {
                            return;
                        }
                    }

                    AssetBundle downloadedBundle = null;

                    if (request.result != UnityWebRequest.Result.ProtocolError)
                    {
                        try
                        {
                            downloadedBundle = DownloadHandlerAssetBundle.GetContent(request);
                        }
                        catch (Exception e)
                        {
                            _logService.Exception(e, "FailedbundleDownload: " + bad.url + " " + bad.assetName);
                        }
                    }

                    if (downloadedBundle != null)
                    {
                        AddBundleToCache(bad, downloadedBundle);

                        request.Dispose();
                        return;
                    }
                    else
                    {
                        request.Dispose();
                        await Awaitable.WaitForSecondsAsync(0.4f);
                        if (!TokenUtils.IsValid(_token) || !TokenUtils.IsValid(bad.Token))
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(_clientAppService.StreamingAssetsPath + "/" + bad.bundleName);

                while (!request.isDone)
                {
                    await Awaitable.NextFrameAsync();
                    if (!TokenUtils.IsValid(_token) || !TokenUtils.IsValid(bad.Token))
                    {
                        return;
                    }
                }


                if (request.assetBundle != null)
                {

                    AddBundleToCache(bad, request.assetBundle);
                    return;
                }
            }
        }
        if (!_bundleFailedDownloads.Contains(bad.bundleName))
        {
            _bundleFailedDownloads.Add(bad.bundleName);
        }
    }


    protected GameObject InstantiateBundledAsset(BundleCacheData bundleCache, GameObject loadedAsset, GameObject parent)
    {

        GameObject go = InstantiateIntoParent(loadedAsset, parent);

        if (go == null)
        {
            _logService.Error("Failed to load asset from " + bundleCache.Name);
            return null;
        }

        bundleCache.LastUsed = DateTime.UtcNow;
        MonoBehaviour mbh = go.GetComponent<MonoBehaviour>();

        if (mbh == null)
        {
            mbh = go.AddComponent<BundleCacheItem>();
        }

        bundleCache.Instances.Add(go);
        _clientEntityService.RegisterDestroyCallback(mbh, () =>
        {
            bundleCache.Instances.Remove(go);
        });
        BaseBehaviour oneBehavior = go.GetComponent<BaseBehaviour>();
        if (oneBehavior != null)
        {
            _clientEntityService.InitializeHierarchy(go);
        }

        return go;
    }

    /// <summary>
    /// Get the bundle name for an asset, leave an override so later on I can have different
    /// categories of asset bundles or different numbers of asset bundles for different
    /// games.
    /// </summary>
    /// <param name="gs"></param>
    /// <param name="assetPath"></param>
    /// <param name="pathPrefix"></param>
    /// <returns></returns>
    /// 
    private Dictionary<string, Dictionary<string, string>> _existingBundleNames = new Dictionary<string, Dictionary<string, string>>();
    public virtual string GetBundleNameForCategoryAndAsset(string pathPrefix, string assetPath)
    {
        if (_existingBundleNames.TryGetValue(pathPrefix, out Dictionary<string, string> assetDictionary))
        {
            if (assetDictionary.TryGetValue(assetPath, out string path))
            {
                return path;
            }
        }
        else
        {
            assetDictionary = new Dictionary<string, string>();
            _existingBundleNames[pathPrefix] = assetDictionary;
        }

        string fullName = pathPrefix + "/" + assetPath;

        int firstSlashIndex = fullName.IndexOf('/');
        int lastSlashIndex = fullName.LastIndexOf('/');

        string endFilename = fullName.Substring(lastSlashIndex + 1);

        // Two slashes, so the fullName becomes everything before the last slash
        if (firstSlashIndex > 0 && lastSlashIndex > firstSlashIndex && lastSlashIndex < fullName.Length - 1)
        {
            fullName = fullName.Substring(0, lastSlashIndex);
        }

        string letterDigitName = new String(fullName.Where(x => char.IsLetterOrDigit(x)).ToArray()).ToLowerInvariant();

        int endDigitsToRemove = 0;

        for (int pos = letterDigitName.Length - 1; pos >= 0; pos--)
        {
            if (char.IsDigit(letterDigitName[pos]))
            {
                endDigitsToRemove++;
            }
            else
            {
                break;
            }
        }

        string finalName = letterDigitName.Substring(0, letterDigitName.Length - endDigitsToRemove);

        assetDictionary[assetPath] = finalName;

        return finalName;
    }

    public void LoadAssetInto<T>(object parent, string assetPathSuffix, string assetPath,
        AssetDownloadHandler<T> handler, CancellationToken token, T data = default(T), string subdirectory = null)
    {
        LoadAsset<T>(assetPathSuffix, assetPath, handler, parent, token, data, subdirectory);
    }

    public string StripPathPrefix(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "";
        }
        path = path.Substring(path.LastIndexOf("/") + 1);
        path = path.Substring(path.LastIndexOf("\\") + 1);
        return path;
    }


    private GameObject InstantiateIntoParent(GameObject child, GameObject parent)
    {
        GameObject go = child as GameObject;
        if (go == null)
        {
            return null;
        }
        go = GameObject.Instantiate<GameObject>(go);

        go.name = go.name.Replace("(Clone)", "");
        go.name = go.name.Replace(AssetConstants.ArtFileSuffix, "");

        if (parent != null)
        {
            _clientEntityService.AddToParent(go, parent);
        }
        return go;
    }
    public async Task<object> LoadAssetAsync(string assetCategory, string assetPath, object parent, CancellationToken token, string subdirectory = null)
    {
        return await LoadAssetAsync<object>(assetCategory, assetPath, parent, token, subdirectory);
    }

    public async Task<T> LoadAssetAsync<T>(string assetCategory, string assetPath, object parent, CancellationToken token, string subdirectory = null) where T : class
    {
        GameObjectContainer cont = new GameObjectContainer();
        LoadAssetInto(parent, assetCategory, assetPath, OnLoadEntityAsync, token, cont, subdirectory);

        while (cont.Entity == null && !cont.FailedLoad)
        {
            await Awaitable.NextFrameAsync();
            if (!TokenUtils.IsValid(token))
            {
                return default(T);
            }
        }

        if (typeof(T) == typeof(object))
        {
            return cont.Entity as T;
        }
        else
        {
            return _clientEntityService.GetComponent<T>(cont.Entity);
        }
    }

    private void OnLoadEntityAsync(GameObject go, GameObjectContainer cont, CancellationToken token)
    {
        cont.Entity = go;

        if (cont.Entity == null)
        {
            cont.FailedLoad = true;
        }
    }

    public void UnloadAsset(object obj)
    {
        if (obj is UnityEngine.Object uobj)
        {
            Resources.UnloadAsset(uobj);
        }
    }
}

