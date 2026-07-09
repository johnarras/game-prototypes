
using OxDb.SharedCore.Utils;
using System.Threading;
using UnityEngine;

public class SetfinalTerrainHeights : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int z = 0; z < _mapProvider.GetMap().GetHhgt(); z++)
            {
                if (x == 0 || x == _mapProvider.GetMap().GetHwid() - 1 || z == 0 || z == _mapProvider.GetMap().GetHhgt() - 1)
                {
                    _md.Heights[x, z] = 0;
                }
                _md.Heights[x, z] = MathUtil.Clamp(0, _md.Heights[x, z], 1);
            }
        }

        _zoneGenService.SetAllHeightmaps(_md.Heights, token);


        _md.HaveSetHeights = true;
    }
}



