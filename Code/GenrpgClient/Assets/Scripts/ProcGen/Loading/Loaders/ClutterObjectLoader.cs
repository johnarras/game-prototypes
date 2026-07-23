
using OxDb.Client.Assets.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Settings.Clutter;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Threading;
using UnityEngine;

public class ClutterObjectLoader : BaseObjectLoader
{
    public override long HelperKey => EntityTypes.Prop;

    public override bool LoadObject(PatchLoadData loadData, int entityId,
       int x, int z, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {
        ClutterType ctype = _gameData.Get<ClutterTypeSettings>(_gs.ch).Get(entityId);
        if (ctype == null)
        {
            return false;
        }

        string artName = ctype.Art;
        int indexHash = loadData.gx * z + loadData.gz * x + x * 13 + loadData.gz * 19 + loadData.gx * 31 + z * 47;
        int indexChoice = 1;
        if (ctype.NumChoices > 0)
        {
            indexChoice = 1 + indexHash % ctype.NumChoices;
        }
        string prefabName = ctype.Art + indexChoice;

        DownloadObjectData dlo = new DownloadObjectData();
        dlo.gameItem = ctype;
        dlo.url = prefabName;
        dlo.loadData = loadData;
        dlo.x = x;
        dlo.z = z;
        dlo.zOffset = RandUtils.FloatRange(0, 1, _gs.Rand);
        dlo.zone = currZone;
        dlo.zoneType = currZoneType;
        dlo.assetCategory = AssetCategoryNames.Props;

        dlo.rotation = new Point3F(((indexHash * 37) % 4) * 90, (indexHash * 23) % 360, ((indexHash * 59) % 4) * 90);

        _assetService.LoadAsset(AssetCategoryNames.Props, dlo.url, OnDownloadObject, null, token, dlo);

        if (indexHash % 3 == 2)
        {
            indexHash = indexHash * 17 + 7;
            dlo = new DownloadObjectData();
            dlo.gameItem = ctype;
            dlo.url = prefabName;
            dlo.loadData = loadData;

            dlo.x = x + ((indexHash / 7) % 3 - 1);
            dlo.z = z + ((indexHash / 131) % 3 - 1);
            dlo.zOffset = RandUtils.FloatRange(0, 1, _gs.Rand);
            dlo.zone = currZone;
            dlo.zoneType = currZoneType;
            dlo.assetCategory = AssetCategoryNames.Props;
            dlo.AfterLoad = AfterLoadObject;

            dlo.rotation = new Point3F(((indexHash * 37) % 4) * 90, (indexHash * 23) % 360, ((indexHash * 59) % 4) * 90);

            _assetService.LoadAsset(AssetCategoryNames.Props, dlo.url, OnDownloadObject, null, token, dlo);


        }
        return true;
    }

    public void AfterLoadObject(GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        float ddscale = 0.5f;

        MeshCollider collider = go.GetComponent<MeshCollider>();

        if (collider != null)
        {
            collider.convex = true;
        }

        go.transform.localPosition = new Vector3(dlo.x + dlo.ddx * ddscale, dlo.height + dlo.zOffset, dlo.z + dlo.ddz * ddscale);
        if (dlo.rotation != null)
        {
            go.transform.eulerAngles = new Vector3(dlo.rotation.X, dlo.rotation.Z, dlo.rotation.Z);
        }
    }
}


