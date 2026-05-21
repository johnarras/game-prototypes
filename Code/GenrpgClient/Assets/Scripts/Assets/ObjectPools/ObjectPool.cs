using Assets.Scripts.Assets.Entities;
using Assets.Scripts.Awaitables;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.GameObjects;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Assets.ObjectPools
{
    public interface IObjectPool : IInitializable, IClientResetCleanup
    {
        void CheckoutObject<T>(object parent, string assetCategory, string assetPath,
            AssetDownloadHandler<T> handler, T data, CancellationToken token, string subdirectory = null);
        Task<GameObject> CheckoutObjectAsync<T>(object parent, string assetCategory, string assetPath,
            AssetDownloadHandler<T> handler, T data, CancellationToken token, string subdirectory = null);
        void ReturnObject(object pooled);
        void Clear();

    }

    public class PrefabCache
    {
        public string Name;
        public List<GameObject> Cache = new List<GameObject>();
        public GameObject Parent;
    }

    public class ObjectPool : IObjectPool
    {
        private ISingletonContainer _singletonContainer = null;
        private IAwaitableService _awaitableService = null;
        private IClientEntityService _clientEntityService = null;

        private GameObject _pooledObjectParent = null;

        private IAssetService _assetService = null;

        private Dictionary<string, PrefabCache> _cache = new Dictionary<string, PrefabCache>();

        private Dictionary<GameObject, PrefabCache> _activeMap = new Dictionary<GameObject, PrefabCache>();

        private CancellationToken _token;
        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _pooledObjectParent = _singletonContainer.GetAssetParent<ObjectPool>();
            await Task.CompletedTask;

        }

        public void Clear()
        {
            foreach (PrefabCache cache in _cache.Values)
            {
                foreach (GameObject go in cache.Cache)
                {
                    _clientEntityService.Destroy(go);
                }
                _clientEntityService.Destroy(cache.Parent);
            }

            foreach (GameObject go in _activeMap.Keys)
            {
                _clientEntityService.Destroy(go);
            }

            _cache.Clear();
            _activeMap.Clear();
        }


        public void ReturnObject(object obj)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                BaseBehaviour bb = obj as BaseBehaviour;

                if (bb != null)
                {
                    go = bb.gameObject;
                }
            }

            IPooledObject pooled = obj as IPooledObject;

            if (pooled != null)
            {
                pooled.OnReturn();
            }

            if (!_activeMap.TryGetValue(go, out PrefabCache cache))
            {
                _clientEntityService.Destroy(go);
                return;
            }

            cache.Cache.Add(go);
            _clientEntityService.SetActive(go, false);
            _clientEntityService.AddToParent(go, cache.Parent);

            _activeMap.Remove(go);

        }

        private PrefabCache GetCache(string key)
        {
            if (!_cache.TryGetValue(key, out PrefabCache currCache))
            {
                currCache = new PrefabCache();
                _cache[key] = currCache;
                currCache.Parent = _pooledObjectParent;
            }
            return currCache;
        }

        public void CheckoutObject<T>(object parent, string assetCategory, string assetPath,
            AssetDownloadHandler<T> handler, T data, CancellationToken token, string subdirectory = null)
        {
            _awaitableService.ForgetTask(CheckoutObjectAsync<T>(parent, assetCategory, assetPath, handler, data, token, subdirectory));
        }

        public async Task<MB> CheckoutObjectAsync<MB, T>(object parent, string assetCategory, string assetPath,
            AssetDownloadHandler<T> handler, T data, CancellationToken token, string subdirectory = null) where MB : MonoBehaviour
        {
            GameObject obj = await CheckoutObjectAsync<T>(parent, assetCategory, assetPath, handler, data, token, subdirectory);

            if (obj == null)
            {
                return default(MB);
            }

            return _clientEntityService.GetComponent<MB>(obj);
        }


        public async Task<GameObject> CheckoutObjectAsync<T>(object parent, string assetCategory, string assetPath,
            AssetDownloadHandler<T> handler, T data, CancellationToken token, string subdirectory = null)
        {

            string fullAssetCategory = assetCategory;
            if (!string.IsNullOrEmpty(subdirectory))
            {
                fullAssetCategory += "/" + subdirectory;
            }

            string bundleName = _assetService.GetBundleNameForCategoryAndAsset(fullAssetCategory, assetPath);

            string fullName = bundleName + assetPath;

            GameObject newItem = null;

            PrefabCache cache = GetCache(fullName);

            if (cache.Cache.Count > 0)
            {
                newItem = cache.Cache[cache.Cache.Count - 1];
                cache.Cache.RemoveAt(cache.Cache.Count - 1);
            }

            if (newItem == null)
            {
                newItem = (GameObject)(await _assetService.LoadAssetAsync(assetCategory, assetPath, parent, token, subdirectory));
            }

            if (newItem == null)
            {
                return null;
            }

            if (parent == null)
            {
                newItem.transform.parent = null;
            }
            else
            {
                _clientEntityService.AddToParent(newItem, parent);
            }

            _clientEntityService.SetActive(newItem, true);

            if (handler != null)
            {
                handler(newItem, data, token);
            }

            _activeMap[newItem] = cache;

            return newItem;
        }

        public async Task OnReset(CancellationToken token)
        {
            Clear();
            await Task.CompletedTask;
        }
    }
}

