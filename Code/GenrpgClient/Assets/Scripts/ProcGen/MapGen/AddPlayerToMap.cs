using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Units.Settings;
using System.Threading;
using UnityEngine;

class AddPlayerToMap : BaseZoneGenerator
{

    protected IUnitSetupService _unitSetupService;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        UnitType utype = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(_gs.ch.EntityId);

        if (utype == null || string.IsNullOrEmpty(utype.Art))
        {
            return;
        }

        _assetService.LoadAssetInto(null, AssetCategoryNames.Monsters, utype.Art, OnLoadPlayer, token, _gs.ch);

    }

    private void OnLoadPlayer(GameObject go, Character ch, CancellationToken token)
    {
        if (go == null || ch == null)
        {
            return;
        }
        SpawnLoadData loadData = new SpawnLoadData()
        {
            Obj = ch,
            Spawn = new OnSpawn(),
            Token = _token,
        };

        GameObject go2 = _unitSetupService.SetupUnit(go, loadData, _token);
        float height = _terrainManager.SampleHeight(ch.X, ch.Z);
        go2.transform.position = new Vector3(ch.X, MapConstants.MapHeight, ch.Z);
        go2.transform.eulerAngles = new Vector3(0, ch.Rot, 0);

    }
}
