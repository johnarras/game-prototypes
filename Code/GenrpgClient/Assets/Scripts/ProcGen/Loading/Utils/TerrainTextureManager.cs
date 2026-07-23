using OxDb.Client.Assets.Constants;
using OxDb.Client.Assets.Services;
using OxDb.Client.Assets.Textures;
using OxDb.Client.Core.Interfaces;
using OxDb.Client.GameObjects;
using OxDb.Client.MapTerrain;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.ProcGen.Settings.Textures;
using OxDb.SharedGame.Zones.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class CachedTextureList
{
    public long TextureTypeId;

    public TextureList TextureList;

    public GameObject TextureListGameObject;

    public Texture2D Diffuse;
    public Texture2D Normal;
    public TerrainLayer TerrLayer;


    public List<GameObject> Instances = new List<GameObject>();

    public bool IsValid()
    {
        return TerrLayer != null && Diffuse != null;
    }
}

public class DownloadTerrainTextureArgs
{
    public IndexedTerrainLayer Indexed;
    public long TextureTypeId;
}

public interface ITerrainTextureManager : IInitializable, IClientResetCleanup, IAssetSubsystem
{
    Awaitable SetupTerrainContainerLayers(ITerrainContainer container, List<long> zoneTypeIdList, List<long> textureTypeIdList, CancellationToken token, bool allAtOnce = false);
    Awaitable DownloadAllTerrainTextures(CancellationToken token);
    TerrainLayer CreateTerrainLayer(Texture2D diffuse, Texture2D normal = null);
    Texture2D GetBasicTerrainTexture(int index);
    void SetupTerrainTexture(IndexedTerrainLayer indexedLayer, CancellationToken token);

}

public class TerrainTextureManager : ITerrainTextureManager
{

    private ILogService _logService = null;
    private IGameData _gameData;
    private IAssetService _assetService = null;
    protected IClientGameState _gs;
    protected IMapGenData _md;
    protected IClientEntityService _clientEntityService = null;
    private ISingletonContainer _singletonContainer = null;

    private GameObject _textureParent = null;

    private CancellationToken _token;

    private Dictionary<long, CachedTextureList> _textureCache = new Dictionary<long, CachedTextureList>();

    public async Task Initialize(CancellationToken token)
    {
        _token = token;

        _textureParent = _singletonContainer.GetAssetParent<TextureList>();

        await Task.CompletedTask;
    }


    public async Task OnReset(CancellationToken token)
    {
        _clientEntityService.DestroyAllChildren(_textureParent);
        _textureCache.Clear();
        await Task.CompletedTask;
    }

    public async Awaitable UpdateAssets(CancellationToken token)
    {
        foreach (CachedTextureList cachedList in _textureCache.Values)
        {
            cachedList.Instances = cachedList.Instances.Where(x => !System.Object.ReferenceEquals(x, null)).ToList();

            if (cachedList.Instances.Count < 1)
            {
                // _textureCache.Remove(cachedList.TextureTypeId);
                // _clientEntityService.Destroy(cachedList.TextureList);
                break;
            }
        }
        await Task.CompletedTask;
    }



    public TerrainLayer CreateTerrainLayer(Texture2D diffuse = null, Texture2D normal = null)
    {
        TerrainLayer tl = new TerrainLayer();
        if (diffuse == null)
        {
            diffuse = new Texture2D(2, 2);
        }

        tl.diffuseTexture = diffuse;
        tl.normalMapTexture = normal;
        InitTerrainLayerData(tl);
        return tl;
    }

    public void InitTerrainLayerData(TerrainLayer tl)
    {
        if (tl == null)
        {
            return;
        }
        tl.normalScale = 1.0f;
        tl.metallic = 0.4f; // Set to 0 if using Standard terrain shader.
        tl.smoothness = 0.4f;
        tl.specular = (UnityEngine.Color.gray * 0.00f);
        tl.tileOffset = new Vector2(MapConstants.TerrainLayerOffset, MapConstants.TerrainLayerOffset);
        tl.tileSize = new Vector2(MapConstants.TerrainLayerTileSize, MapConstants.TerrainLayerTileSize);
    }


    public Texture2D[] _basicTerrainTextures = null;

    public Texture2D GetBasicTerrainTexture(int index)
    {
        if (_basicTerrainTextures == null)
        {
            UnityEngine.Color[] colors = new UnityEngine.Color[] { Color.green * 0.6f, new UnityEngine.Color(0.6f, 0.3f, 0), Color.white * 0.4f, Color.white * 0.8f };
            _basicTerrainTextures = new Texture2D[TerrainTexChannels.Max];
            for (int c = 0; c < colors.Length && c < TerrainTexChannels.Max; c++)
            {

                Texture2D tex = new Texture2D(4, 4, TextureFormat.ARGB32, false, true);
                Color[] texColors = tex.GetPixels();
                for (int i = 0; i < texColors.Length; i++)
                {
                    texColors[i] = colors[c];
                }
                tex.SetPixels(texColors);
                tex.Apply();
                _basicTerrainTextures[c] = tex;
            }
        }

        if (index < 0 || index >= _basicTerrainTextures.Length)
        {
            return new Texture2D(2, 2);
        }
        return _basicTerrainTextures[index];
    }

    public async Awaitable SetupTerrainContainerLayers(ITerrainContainer cont, List<long> zoneTypeIdList, List<long> textureTypeIdList, CancellationToken token, bool allAtOnce = false)
    {
        Terrain terr = cont.Core.Terrain;
        if (terr == null || terr.terrainData == null)
        {
            return;
        }

        if (zoneTypeIdList == null || zoneTypeIdList.Count < 1)
        {
            zoneTypeIdList = new List<long>();
            zoneTypeIdList.Add(1);
        }
        cont.Core.Layers = new List<IndexedTerrainLayer>();

        foreach (long zoneTypeId in zoneTypeIdList)
        {
            ZoneType zoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zoneTypeId);

            for (int i = 0; i < TerrainTexChannels.Max; i++)
            {
                cont.Core.AddNewTextureIndex(zoneType.GetTerrainTextureIdFromChannel(i), zoneType.IdKey);
            }
        }
        foreach (long textureTypeId in textureTypeIdList)
        {
            cont.Core.AddNewTextureIndex(textureTypeId, 0);
        }

        List<long> textureTypeIds = cont.Core.Layers.Select(x => x.TextureTypeId).ToList();

        TerrainLayer[] terrainLayers = new TerrainLayer[cont.Core.Layers.Count];

        for (int i = 0; i < terrainLayers.Length; i++)
        {
            terrainLayers[i] = new TerrainLayer();
        }

        terr.terrainData.terrainLayers = terrainLayers;

        await DownloadTextureLayers(cont, allAtOnce, token);
    }

    private async Awaitable DownloadTextureLayers(ITerrainContainer cont, bool allAtOnce, CancellationToken token)
    {
        for (int l = 0; l < cont.Core.Layers.Count; l++)
        {
            if (!allAtOnce)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }

            SetupTerrainTexture(cont.Core.Layers[l], token);

        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gs"></param>
    /// <param name="terr"></param>
    /// <param name="textureId"></param>
    /// <param name="index">current index taking into account the zone offset of 4 per zone</param>
	public void SetupTerrainTexture(IndexedTerrainLayer indexedLayer, CancellationToken token)
    {
        CachedTextureList textureData = GetTerrainTextureCacheData(indexedLayer.TextureTypeId);
        if (textureData != null)
        {
            SetNewTerrainLayer(indexedLayer, textureData);
            return;
        }

        TextureType textureType = _gameData.Get<TextureTypeSettings>(_gs.ch).Get(indexedLayer.TextureTypeId);
        if (textureType == null)
        {
            _logService.Info("TextureType is null: TextureId: " + indexedLayer.TextureTypeId + " Index: " + indexedLayer.Index);
            textureType = _gameData.Get<TextureTypeSettings>(_gs.ch).Get(1);
        }

        string artName = textureType.Art;

        DownloadTerrainTextureArgs newDownloadData = new DownloadTerrainTextureArgs();
        newDownloadData.TextureTypeId = textureType.IdKey;
        newDownloadData.Indexed = indexedLayer;

        _assetService.LoadAssetInto(_textureParent, AssetCategoryNames.TextureLists, artName, OnDownloadTextureList, token, newDownloadData);
    }


    private void SetNewTerrainLayer(IndexedTerrainLayer layer, CachedTextureList cachedTexture)
    {

        if (!cachedTexture.IsValid())
        {
            return;
        }
        layer.TerrainLayer = cachedTexture.TerrLayer;

        if (layer.Core == null || !layer.Core.IsValid())
        {
            return;
        }

        TerrainLayer[] currLayers = layer.Core.TerrainData.terrainLayers;

        if (currLayers == null || layer.Index < 0 || layer.Index >= currLayers.Length)
        {
            return;
        }

        currLayers[layer.Index] = cachedTexture.TerrLayer;

        if (!cachedTexture.Instances.Contains(layer.Core.Terrain.gameObject))
        {
            cachedTexture.Instances.Add(layer.Core.Terrain.gameObject);
        }

        layer.Core.TerrainData.terrainLayers = currLayers;
        layer.Core.Terrain.Flush();
    }

    private CachedTextureList GetTerrainTextureCacheData(long textureTypeId)
    {

        if (_textureCache.TryGetValue(textureTypeId, out CachedTextureList tlist))
        {
            return tlist;
        }
        return null;
    }


    public async Awaitable DownloadAllTerrainTextures(CancellationToken token)
    {
        foreach (TextureType textureType in _gameData.Get<TextureTypeSettings>(_gs.ch).GetData())
        {
            DownloadTerrainTextureArgs newDownloadData = new DownloadTerrainTextureArgs() { TextureTypeId = textureType.IdKey };

            _assetService.LoadAssetInto(_textureParent, AssetCategoryNames.TextureLists, textureType.Name, OnDownloadTextureList, token, newDownloadData);
        }

        await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);

        while (_assetService.IsDownloading())
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }

        await Awaitable.WaitForSecondsAsync(0.1f * _gameData.Get<TextureTypeSettings>(_gs.ch).GetData().Count, cancellationToken: token);
    }


    private void OnDownloadTextureList(GameObject go, DownloadTerrainTextureArgs ddata, CancellationToken token)
    {

        if (go == null)
        {
            return;
        }

        if (ddata == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        CachedTextureList currentList = GetTerrainTextureCacheData(ddata.TextureTypeId);

        if (currentList != null)
        {
            _clientEntityService.Destroy(go);
        }
        else
        {
            TextureList texList = go.GetComponent<TextureList>();

            if (texList == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            currentList = new CachedTextureList()
            {
                TextureTypeId = ddata.TextureTypeId,
                TextureList = texList,
                Diffuse = texList.Diffuse,
                Normal = texList.Normal,
                TextureListGameObject = texList.gameObject,
            };

            currentList.TerrLayer = CreateTerrainLayer(currentList.Diffuse, currentList.Normal);

            _textureCache[ddata.TextureTypeId] = currentList;
        }

        if (ddata.Indexed != null)
        {
            SetNewTerrainLayer(ddata.Indexed, currentList);
        }
    }
}



