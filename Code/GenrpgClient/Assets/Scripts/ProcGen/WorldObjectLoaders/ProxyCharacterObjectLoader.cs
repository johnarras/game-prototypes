
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using System.Threading;
using UnityEngine;

public class ProxyCharacterObjectLoader : UnitObjectLoader
{
    public override long HelperKey => EntityTypes.ProxyCharacter;

    public override async Awaitable Load(OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        await base.Load(spawn, obj, token);
    }
    protected override void AfterLoadUnit(GameObject go, SpawnLoadData loadData, CancellationToken token)
    {
        base.AfterLoadUnit(go, loadData, token);
    }
}


