using OxDb.Client.Assets.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Settings.Bridges;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Threading;
using UnityEngine;

public class BridgeObjectLoader : BaseObjectLoader
{
    public override long HelperKey => EntityTypes.Bridge;

    public override bool LoadObject(PatchLoadData loadData, int entityId,
       int x, int z, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {

        BridgeType bridgeType = _gameData.Get<BridgeTypeSettings>(_gs.ch).Get(entityId);
        if (bridgeType == null)
        {
            return false;
        }

        ExtendedWorldObjectData extData = loadData.patch.GetObjAtPos(loadData, x, z);

        if (extData == null)
        {
            return false;
        }

        string prefabName = bridgeType.Art;

        DownloadObjectData dlo = new DownloadObjectData();
        dlo.gameItem = bridgeType;
        dlo.url = prefabName;
        dlo.loadData = loadData;
        dlo.x = x;
        dlo.z = z;
        dlo.finalY = extData.Height;
        dlo.zone = currZone;
        dlo.zoneType = currZoneType;
        dlo.assetCategory = AssetCategoryNames.Props;

        dlo.rotation = new Point3F(0, extData.Angle, 0);
        dlo.AfterLoad = AfterLoadObject;

        _assetService.LoadAsset(AssetCategoryNames.Props, dlo.url, OnDownloadObject, null, token, dlo);

        return true;
    }
    public void AfterLoadObject(GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
        if (dlo.rotation != null)
        {
            go.transform.Rotate(dlo.rotation.X, dlo.rotation.Z, dlo.rotation.Z);
        }
    }
}


