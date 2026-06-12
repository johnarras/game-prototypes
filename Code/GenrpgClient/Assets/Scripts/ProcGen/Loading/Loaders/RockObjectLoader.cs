
using Assets.Scripts.Assets.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.ProcGen.Settings.Rocks;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Threading;
using UnityEngine;

public class RockObjectLoader : BaseObjectLoader
{
    public override long HelperKey => EntityTypes.Rock;

    public override bool LoadObject(PatchLoadData loadData, int entityId,
        int x, int y, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {
        RockType rockType = _gameData.Get<RockTypeSettings>(_gs.ch).Get(entityId);
        if (rockType == null || rockType.Art == null)
        {
            return false;
        }

        int indexHash = (loadData.gx * 113 + loadData.gy * 317 + x * 59 + y * 3141) % rockType.MaxIndex;

        int index = 0;
        if (rockType.MaxIndex > 0)
        {
            index = (indexHash / 2) % rockType.MaxIndex;
        }

        string artName = rockType.Art + (indexHash.ToString("D3"));

        bool smallObject = ((indexHash * 5) % 3 == 0);

        DownloadObjectData dlo = new DownloadObjectData();
        dlo.gameItem = rockType;
        dlo.url = artName;
        dlo.loadData = loadData;
        dlo.x = x;
        dlo.y = y;
        dlo.zone = currZone;
        dlo.zoneType = currZoneType;
        dlo.assetCategory = AssetCategoryNames.Rocks;
        dlo.data = (smallObject ? "small" : "");
        dlo.AfterLoad = AfterLoadRock;

        _assetService.LoadAsset(AssetCategoryNames.Rocks, artName, OnDownloadObject, null, token, dlo);

        return true;

    }

    public void AfterLoadRock(GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        if (go == null || dlo == null)
        {
            return;
        }

        float minScale = 0.8f;
        float maxScale = 2.0f;

        if (dlo.data != null && dlo.data.ToString() == "small")
        {
            minScale *= 0.3f;
            maxScale *= 0.6f;

            if (dlo.placementSeed % 17 == 5)
            {
                minScale *= 1.0f;
                maxScale *= 1.4f;
            }
        }

        float newScale = RandUtils.SeedFloatRange(dlo.placementSeed, 147, minScale, maxScale);



        go.transform.localScale = new Vector3(newScale, newScale, newScale);

        float xrot = RandUtils.SeedFloatRange(dlo.placementSeed, 103, 0, 359, 360);
        float yrot = RandUtils.SeedFloatRange(dlo.placementSeed, 461, 0, 359, 360);
        float zrot = RandUtils.SeedFloatRange(dlo.placementSeed, 2767, 0, 359, 360);



        go.transform.Rotate(xrot, yrot, zrot);


        //go.transform.position = Gnew Vector3(dlo.x, go.transform.position.y, dlo.y);
    }
}

