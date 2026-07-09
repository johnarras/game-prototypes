using Assets.Scripts.Assets.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Constants;
using OxDb.SharedGame.ProcGen.Settings.Trees;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TreeObjectLoader : BaseObjectLoader
{
    public override long HelperKey => EntityTypes.Tree;
    const int ScaleStepCount = 20;

    public override bool LoadObject(PatchLoadData loadData, int entityId,
        int x, int z, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {
        FullTreePrototype fullProto = null;
        TreeType treeType = null;

        if (loadData == null || loadData.terrManager == null)
        {
            return false;
        }

        SetupZoneTreeCache(_gs);

        string assetCategory = AssetCategoryNames.Trees;

        treeType = _gameData.Get<TreeTypeSettings>(_gs.ch).Get(entityId);

        if (treeType == null)
        {
            return false;
        }

        if (_mapProvider.GetMap().OverrideZoneId > 0 && _mapProvider.GetMap().OverrideZonePercent > 0)
        {
            if (loadData.patch.overrideZoneScales[x, z] < _mapProvider.GetMap().OverrideZonePercent)
            {
                Zone zone = _mapProvider.GetMap().Get<Zone>(_mapProvider.GetMap().OverrideZoneId);
                if (zone != null)
                {
                    List<long> okTreeIds = new List<long>();

                    if (_md.zoneTreeIds.TryGetValue(zone.ZoneTypeId, out List<long> treeIds))
                    {
                        okTreeIds = treeIds;
                    }

                    if (okTreeIds.Count > 0)
                    {
                        long treeTypeId = okTreeIds[(loadData.gx * 191 + loadData.gz * 2189 + x * 108061 + z * 857) % okTreeIds.Count];

                        TreeType treeType2 = _gameData.Get<TreeTypeSettings>(_gs.ch).Get(treeTypeId);

                        if (treeType2 != null)
                        {
                            treeType = treeType2;
                        }

                    }
                }
            }
        }


        long index = GetIndexForTree(currZone, treeType, loadData.gx * z + loadData.gz * x + x * 11 + z * 31);
        string artName = treeType.Art + index;
        if (false && treeType.HasFlag(TreeFlags.DirectPlaceObject))
        {
            DownloadObjectData dlo = new DownloadObjectData();
            dlo.gameItem = treeType;
            dlo.url = artName;
            dlo.loadData = loadData;
            dlo.x = x;
            dlo.z = z;
            dlo.zone = currZone;
            dlo.zoneType = currZoneType;
            dlo.assetCategory = assetCategory;


            long placementSeed = 17041 + x * 9479 + z * 2281 + loadData.gx * 5281 + loadData.gz * 719 +
                loadData.gx * z + loadData.gz * x;

            treeType.Scale = 1.0f; // TODO Fix
            float minScale = treeType.Scale;
            float maxScale = treeType.Scale * 1.50f;
            float finalScale = minScale + (maxScale - minScale) * (placementSeed % (ScaleStepCount + 1)) / ScaleStepCount;

            finalScale *= AddTrees.TreeSizeScale;

            dlo.scale = finalScale;

            _assetService.LoadAsset(assetCategory, artName, OnDownloadObjectDirect, null, token, dlo);

        }
        else
        {
            fullProto = new FullTreePrototype();
            fullProto.treeType = treeType;


            StartPlaceInstance(loadData, treeType, assetCategory, artName, x, z, null, token);
        }
        return true;
    }

    protected void OnDownloadObjectDirect(GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        OnDownloadObject(go, dlo, token);
    }


    public long GetIndexForTree(Zone zone, TreeType treeType, int localSeed)
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
        bool isbush = false;
        TreeType tt = dataItem as TreeType;
        if (tt != null)
        {

            tt.Scale = 1.0f; // TODO Fix
            float minScale = tt.Scale;
            float maxScale = minScale * 1.5f;

            maxScale *= AddTrees.TreeSizeScale;

            float finalScale = minScale + (maxScale - minScale) * (placementSeed % (ScaleStepCount + 1)) / ScaleStepCount;

            Vector3 currNormal = _terrainManager.GetInterpolatedNormal(_mapProvider.GetMap(), wx, wz);

            float offsetScale = 1.0f;

            Vector3 offset = currNormal * -(0.3f + (2.5f * (1 - currNormal.y)) / Math.Max(1.0f, offsetScale));
            ex += offset.x;
            ey += offset.y - 1.0f;
            ez += offset.z;

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

        List<MeshRenderer> renderers = _clientEntityService.GetComponents<MeshRenderer>(go);

        foreach (MeshRenderer renderer in renderers)
        {
            List<Material> oldList = new List<Material>();
            renderer.GetMaterials(oldList);
            foreach (Material mat in oldList)
            {
            }
        }

        go = op.terrManager.AddOrReuseTerrainProtoObject(op.Name, go);


        op.Prototype.prefab = go;

        _clientEntityService.SetLayer(go, LayerNames.ObjectLayer);
        go.transform.localPosition = new Vector3(0, -2000, 0);
    }

    private void SetupZoneTreeCache(IClientGameState gs)
    {
        if (_md.zoneTreeIds != null && _md.zoneBushIds != null)
        {
            return;
        }

        _md.zoneTreeIds = new Dictionary<long, List<long>>();
        _md.zoneBushIds = new Dictionary<long, List<long>>();

        TreeTypeSettings treeSettings = _gameData.Get<TreeTypeSettings>(gs.ch);
        foreach (IGameSettings settings in _gameData.Get<ZoneTypeSettings>(gs.ch).GetChildren())
        {
            if (settings is ZoneType zoneType)
            {
                List<long> treeList = new List<long>();
                List<long> bushList = new List<long>();

                _md.zoneTreeIds[zoneType.IdKey] = treeList;
                _md.zoneBushIds[zoneType.IdKey] = bushList;

                foreach (ZoneTreeType ztt in zoneType.TreeTypes)
                {
                    TreeType treeType = treeSettings.Get(ztt.TreeTypeId);

                    treeList.Add(treeType.IdKey);
                }
            }
        }

    }
}


