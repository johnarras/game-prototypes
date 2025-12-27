
using System.Threading;
using UnityEngine;

public class AddNPCs : BaseZoneGenerator
{
    protected IMapGenService _mapGenService = null;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        _mapGenService.AddNPCs(_gs);
    }
}

