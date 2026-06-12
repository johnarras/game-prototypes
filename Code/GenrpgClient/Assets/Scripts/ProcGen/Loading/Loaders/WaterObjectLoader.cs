
using Assets.Scripts.Assets.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Constants;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Linq;
using System.Threading;
using UnityEngine;

public class WaterObjectLoader : BaseObjectLoader
{
    public override long HelperKey => EntityTypes.Water;

    public override bool LoadObject(PatchLoadData loadData, int entityId,
        int x, int y, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {

        if (loadData.patch == null)
        {
            return false;
        }

        ExtendedWorldObjectData extData = loadData.patch.ExtendedObjects.FirstOrDefault(e => e.X == x && e.Z == y);

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
        dlo.y = y;
        dlo.finalZ = heightOffset - 0.5f;
        dlo.zone = currZone;
        dlo.zoneType = currZoneType;
        dlo.assetCategory = AssetCategoryNames.Prefabs;
        dlo.data = new MyPointF(xSize, heightOffset, zSize);

        _assetService.LoadAsset(AssetCategoryNames.Prefabs, artName, OnDownloadWater, null, token, dlo);

        return true;

    }
    public virtual void OnDownloadWater(GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        MyPointF size = dlo.data as MyPointF;
        if (size == null)
        {
            return;
        }

        int gx = dlo.loadData.gx;
        int gy = dlo.loadData.gy;
        int wx = gx * (MapConstants.TerrainPatchSize - 1) + dlo.x;
        int wy = gy * (MapConstants.TerrainPatchSize - 1) + dlo.y;

        if (dlo.loadData.patch == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        Terrain terr = dlo.loadData.patch.terrain as Terrain;
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
        go.transform.localPosition = new Vector3(dlo.x, dlo.finalZ, dlo.y);
        go.transform.localScale = new Vector3(size.X * mult, 1, size.Z * mult);
        _clientEntityService.SetLayer(go, LayerUtils.NameToLayer(LayerNames.Water));




    }
}

