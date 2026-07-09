
using Assets.Scripts.Assets.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Constants;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Threading;
using UnityEngine;

public class WaterObjectLoader : BaseObjectLoader
{
    public override long HelperKey => EntityTypes.Water;

    public override bool LoadObject(PatchLoadData loadData, int entityId,
        int x, int z, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {

        if (loadData.patch == null)
        {
            return false;
        }

        ExtendedWorldObjectData extData = loadData.patch.GetObjAtPos(loadData, x, z);

        if (extData == null)
        {
            return false;
        }

        float heightOffset = extData.Height + MapConstants.MinLandHeight;

        int xSize = extData.XSize;
        int zSize = extData.ZSize;

        string artName = MapConstants.WaterName;
        if (currZone == null)
        {
            artName = MapConstants.MinimapWaterName;
        }

        DownloadObjectData dlo = new DownloadObjectData();
        dlo.url = artName;
        dlo.loadData = loadData;
        dlo.x = x;
        dlo.z = z;
        dlo.finalY = heightOffset - 0.5f;
        dlo.zone = currZone;
        dlo.zoneType = currZoneType;
        dlo.assetCategory = AssetCategoryNames.Prefabs;
        dlo.data = new Point3F(xSize, heightOffset, zSize);

        _assetService.LoadAsset(AssetCategoryNames.Prefabs, artName, OnDownloadWater, null, token, dlo);

        return true;

    }
    public virtual void OnDownloadWater(GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        Point3F size = dlo.data as Point3F;
        if (size == null)
        {
            return;
        }

        int gx = dlo.loadData.gx;
        int gz = dlo.loadData.gz;
        int wx = gx * (MapConstants.TerrainPatchSize - 1) + dlo.x;
        int wz = gz * (MapConstants.TerrainPatchSize - 1) + dlo.z;

        if (dlo.loadData.patch == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        Terrain terr = dlo.loadData.patch.Core.Terrain;
        if (terr != null)
        {
            _clientEntityService.AddToParent(go, terr.gameObject);
        }
        else
        {
            _clientEntityService.Destroy(go);
            return;
        }

        float mult = 2.0f; // = 100.0f // if AQUAS
        go.transform.localPosition = new Vector3(dlo.x, dlo.finalY, dlo.z);
        go.transform.localScale = new Vector3(size.X * mult, 1, size.Z * mult);
        _clientEntityService.SetLayer(go, LayerUtils.NameToLayer(LayerNames.Water));




    }
}

