
using Assets.Scripts.Assets.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Settings.Fences;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Linq;
using System.Threading;
using UnityEngine;

public class FenceObjectLoader : BaseObjectLoader
{

    public override long HelperKey => EntityTypes.Fence;

    public override bool LoadObject(PatchLoadData loadData, int entityId,
        int x, int y, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {

        FenceType fenceType = _gameData.Get<FenceTypeSettings>(_gs.ch).Get((int)entityId);
        if (fenceType == null)
        {
            return false;
        }

        ExtendedWorldObjectData extData = loadData.patch.ExtendedObjects.FirstOrDefault(e => e.X == x && e.Z == y);

        if (extData == null)
        {
            return false;
        }


        string artName = fenceType.Art;

        DownloadObjectData dlo = new DownloadObjectData();
        dlo.gameItem = fenceType;
        dlo.url = artName;
        dlo.loadData = loadData;
        dlo.x = x;
        dlo.y = y;
        dlo.zone = currZone;
        dlo.zoneType = currZoneType;
        dlo.assetCategory = AssetCategoryNames.Props;
        dlo.allowRandomPlacement = false;
        dlo.rotation = new MyPointF(0, extData.Angle, extData.HAngle);
        dlo.AfterLoad = AfterLoadObject;

        _assetService.LoadAsset(AssetCategoryNames.Props, dlo.url, OnDownloadObject, null, token, dlo);

        return true;
    }
    public void AfterLoadObject(GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localPosition += Vector3.up;
        if (dlo.rotation != null)
        {
            go.transform.Rotate(dlo.rotation.X, dlo.rotation.Y, dlo.rotation.Z);
        }
    }
}


