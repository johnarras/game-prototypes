

using OxDb.SharedGame.ProcGen.Settings.Locations;
using OxDb.SharedGame.Zones.WorldData;
using System.Threading;
using UnityEngine;

public class SetMapSpawnPoint : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        int minx = _mapProvider.GetMap().GetHwid() / 2;
        int minz = _mapProvider.GetMap().GetHhgt() / 2;
        long minDist = 1000000000;
        Zone minZone = null;
        int edgeSize = MapConstants.LocCenterEdgeSize;
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            foreach (Location loc in zone.Locations)
            {
                if (loc.CenterX < edgeSize || loc.CenterX >= _mapProvider.GetMap().GetHwid() - edgeSize ||
                    loc.CenterZ < edgeSize || loc.CenterZ >= _mapProvider.GetMap().GetHhgt() - edgeSize)
                {
                    continue;
                }


                long currDist = loc.CenterX * loc.CenterX + loc.CenterZ * loc.CenterZ;
                if (currDist < minDist)
                {
                    minDist = currDist;
                    minx = loc.CenterX;
                    minz = loc.CenterZ;
                    minZone = zone;
                }
            }
        }
        _mapProvider.GetMap().SpawnX = minz;
        _mapProvider.GetMap().SpawnY = minx;
        if (minZone != null)
        {
            minZone.Level = 1;
        }

    }
}



