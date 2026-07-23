using OxDb.Client.Assets.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Constants;
using OxDb.SharedGame.ProcGen.Settings.Trees;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BushObjectLoader : BaseObjectLoader
{
    public override long HelperKey => EntityTypes.Bush;
    const int ScaleStepCount = 20;

    public override bool LoadObject(PatchLoadData loadData, int entityId,
        int x, int z, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {
        FullBushPrototype fullProto = null;
        BushType bushType = null;

        if (loadData == null || loadData.terrManager == null)
        {
            return false;
        }

        SetupZoneBushCache(_gs);

        string assetCategory = AssetCategoryNames.Bushes;

        bushType = _gameData.Get<BushTypeSettings>(_gs.ch).Get(entityId);

        if (bushType == null)
        {
            return false;
        }

        if (!bushType.HasFlag(BushFlags.IsWaterItem) &&
            _mapProvider.GetMap().OverrideZoneId > 0 && _mapProvider.GetMap().OverrideZonePercent > 0)
        {
            if (loadData.patch.overrideZoneScales[x, z] < _mapProvider.GetMap().OverrideZonePercent)
            {
                Zone zone = _mapProvider.GetMap().Get<Zone>(_mapProvider.GetMap().OverrideZoneId);
                if (zone != null)
                {
                    List<long> okBushIds = new List<long>();

                    if (_md.zoneBushIds.TryGetValue(zone.ZoneTypeId, out List<long> bushIds))
                    {
                        okBushIds = bushIds;
                    }

                    if (okBushIds.Count > 0)
                    {
                        long bushTypeId = okBushIds[(loadData.gx * 191 + loadData.gz * 2189 + x * 108061 + z * 857) % okBushIds.Count];

                        BushType bushType2 = _gameData.Get<BushTypeSettings>(_gs.ch).Get(bushTypeId);

                        if (bushType2 != null)
                        {
                            bushType = bushType2;
                        }

                    }
                }
            }
        }


        long index = GetIndexForBush(currZone, bushType, loadData.gx * z + loadData.gz * x + x * 11 + z * 31);
        string artName = bushType.Art + index;
        if (false && bushType.HasFlag(BushFlags.DirectPlaceObject))
        {
            DownloadObjectData dlo = new DownloadObjectData();
            dlo.gameItem = bushType;
            dlo.url = artName;
            dlo.loadData = loadData;
            dlo.x = x;
            dlo.z = z;
            dlo.zone = currZone;
            dlo.zoneType = currZoneType;
            dlo.assetCategory = assetCategory;


            long placementSeed = 17041 + x * 9479 + z * 2281 + loadData.gx * 5281 + loadData.gz * 719 +
                loadData.gx * z + loadData.gz * x;

            bushType.Scale = 1.0f; // TODO Fix
            float minScale = bushType.Scale;
            float maxScale = bushType.Scale * 1.50f;
            float finalScale = minScale + (maxScale - minScale) * (placementSeed % (ScaleStepCount + 1)) / ScaleStepCount;

            finalScale *= AddBushes.BushSizeScale;
            dlo.scale = finalScale;

            _assetService.LoadAsset(assetCategory, artName, OnDownloadObjectDirect, null, token, dlo);

        }
        else
        {
            fullProto = new FullBushPrototype();
            fullProto.treeType = bushType;


            StartPlaceInstance(loadData, bushType, assetCategory, artName, x, z, null, token);
        }
        return true;
    }

    protected void OnDownloadObjectDirect(GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        OnDownloadObject(go, dlo, token);
    }


    public long GetIndexForBush(Zone zone, BushType treeType, int localSeed)
    {
        if (zone == null || treeType == null)
        {
            return 1;
        }

        if (treeType.VariationCount > 1)
        {
            return 1 + (zone.Seed % 100000000 + treeType.IdKey * 12 + treeType.IdKey * treeType.IdKey + localSeed * 13 + localSeed * treeType.IdKey * 17) % treeType.VariationCount;
        }
        return 1;

    }



    protected void StartPlaceInstance(PatchLoadData loadData,
        IIndexedGameItem dataItem,
        string assetCategory, string artName, int x, int z, object extraData, CancellationToken token)
    {
        if (string.IsNullOrEmpty(artName) || loadData == null)
        {
            return;
        }

        string key = artName + assetCategory;

        TreePrototype proto = null;
        int protoIndex = -1;

        for (int i = 0; i < loadData.objectProtos.Count; i++)
        {
            if (loadData.objectProtos[i].Name == key)
            {
                proto = loadData.objectProtos[i].Prototype as TreePrototype;
                protoIndex = i;
                break;
            }
        }

        if (proto == null || protoIndex < 0)
        {
            TreePrototype tp = new TreePrototype();
            ObjectPrototype op = new ObjectPrototype()
            {
                Name = key,
                Prototype = tp,
                DataItem = dataItem,
                terrManager = loadData.terrManager,
                token = token,
            };
            loadData.objectProtos.Add(op);
            proto = tp;
            protoIndex = loadData.objectProtos.Count - 1;


            loadData.terrManager.AddTerrainProtoPatch(artName, loadData.gx, loadData.gz);
            GameObject currObject = loadData.terrManager.GetTerrainProtoObject(artName);

            if (currObject != null)
            {
                tp.prefab = currObject;
                PlaceInstance(dataItem, loadData.treeInstances, protoIndex, loadData.gx, loadData.gz, x, z, extraData);
            }
            else
            {
                _assetService.LoadAsset(assetCategory, artName, OnDownloadPrototype, loadData.protoParent, token, op);
            }
        }

        PlaceInstance(dataItem, loadData.treeInstances, protoIndex, loadData.gx, loadData.gz, x, z, extraData);
    }



    private void PlaceInstance(IIndexedGameItem dataItem, List<TreeInstance> instances, int protoIndex, int gx, int gz, int x, int z, object data)
    {

        long placementSeed = 17041 + x * 9479 + z * 2281 + gx * 5281 + gz * 719 +
            gx * z + gz * x;

        int wx = gx * (MapConstants.TerrainPatchSize - 1) + x;
        int wz = gz * (MapConstants.TerrainPatchSize - 1) + z;
        float ddx = RandUtils.SeedFloatRange(placementSeed * 13, 143, -0.5f, 0.5f, 101);
        float ddz = RandUtils.SeedFloatRange(placementSeed * 17, 149, -0.5f, 0.5f, 101);
        float height = _terrainManager.SampleHeight(wx, wz);

        TreeInstance ti = new TreeInstance();
        ti.prototypeIndex = protoIndex;


        float ex = x + ddx;
        float ey = height;
        float ez = z + ddz;
        bool isbush = true;
        BushType tt = dataItem as BushType;
        if (tt != null)
        {

            tt.Scale = 1.0f; // TODO Fix
            float minScale = 1.0f;
            float maxScale = 1.0f;

            float finalScale = minScale + (maxScale - minScale) * (placementSeed % (ScaleStepCount + 1)) / ScaleStepCount;

            Vector3 currNormal = _terrainManager.GetInterpolatedNormal(_mapProvider.GetMap(), wx, wz);

            ti.heightScale = finalScale;
            ti.widthScale = finalScale;
            ti.rotation = (placementSeed * 1.7f);

        }
        float posMult = 1.0f / (MapConstants.TerrainPatchSize - 1);
        float extraDepth = (isbush ? 0 : 1.5f);
        ti.position = new Vector3(ex * posMult, (ey - extraDepth) / MapConstants.MapHeight, ez * posMult);
        instances.Add(ti);
    }


    private void OnDownloadPrototype(GameObject go, ObjectPrototype op, CancellationToken token)
    {
        if (op == null || op.Prototype == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        go = op.terrManager.AddOrReuseTerrainProtoObject(op.Name, go);

        op.Prototype.prefab = go;

        BushType bushType = op.DataItem as BushType;
        if (bushType != null)
        {
            LODGroup lodGroup = go.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                for (int c = 0; c < go.transform.childCount; c++)
                {
                    Transform child = go.transform.GetChild(c);
                    if (child != null && child.gameObject != null && child.name.IndexOf("LOD0") < 0)
                    {

                        child.gameObject.SetActive(false);
                    }
                }
                lodGroup.enabled = false;
                lodGroup.animateCrossFading = false;
                lodGroup.fadeMode = LODFadeMode.None;
            }
        }

        _clientEntityService.SetLayer(go, LayerNames.ObjectLayer);
        go.transform.localPosition = new Vector3(0, -2000, 0);
    }

    private void SetupZoneBushCache(IClientGameState gs)
    {
        if (_md.zoneBushIds != null && _md.zoneBushIds != null)
        {
            return;
        }

        _md.zoneBushIds = new Dictionary<long, List<long>>();
        _md.zoneBushIds = new Dictionary<long, List<long>>();

        BushTypeSettings bushSettings = _gameData.Get<BushTypeSettings>(gs.ch);
        foreach (IGameSettings settings in _gameData.Get<ZoneTypeSettings>(gs.ch).GetChildren())
        {
            if (settings is ZoneType zoneType)
            {
                List<long> bushList = new List<long>();

                _md.zoneBushIds[zoneType.IdKey] = bushList;

                List<WeightedEntity> zoneBushes = zoneType.GetPropsOfType(EntityTypes.Bush);

                foreach (WeightedEntity zoneBush in zoneBushes)
                {
                    BushType bushType = bushSettings.Get(zoneBush.EntityId);

                    if (bushType.HasFlag(BushFlags.IsWaterItem))
                    {
                        continue;
                    }

                    bushList.Add(bushType.IdKey);
                }
            }
        }
    }
}


