
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
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


