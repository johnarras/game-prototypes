
using OxDb.SharedGame.ProcGen.Entities;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SetupNearbyZones : BaseAddMountains
{

    protected IMapGenService _mapGenService = null;
    public override async Awaitable Generate(CancellationToken token)
    {


        foreach (ConnectedPairData conn in _md.ZoneConnections)
        {
            if (conn.Point1 == null || conn.Point2 == null)
            {
                continue;
            }

            int sx = (int)conn.Point1.X;
            int sz = (int)conn.Point1.Z;
            int ex = (int)conn.Point2.X;
            int ez = (int)conn.Point2.Z;

            if (sx < 0 || sz < 0 || ex < 0 || ez < 0 ||
                sx >= _mapProvider.GetMap().GetHwid() || sz >= _mapProvider.GetMap().GetHhgt() ||
                ex >= _mapProvider.GetMap().GetHwid() || ez >= _mapProvider.GetMap().GetHhgt())
            {
                continue;
            }

            short zoneId1 = _md.MapZoneIds[sx, sz];
            short zoneId2 = _md.MapZoneIds[ex, ez];

            if (zoneId1 != zoneId2)
            {
                Zone zone1 = _mapProvider.GetMap().Get<Zone>(zoneId1);
                Zone zone2 = _mapProvider.GetMap().Get<Zone>(zoneId2);

                GenZone genZone1 = _md.GetGenZone(zone1.IdKey);
                GenZone genZone2 = _md.GetGenZone(zone2.IdKey);

                if (zone1 != null && zone2 != null)
                {
                    int xmid1 = (zone1.MinX + zone1.MaxX) / 2;
                    int zmid1 = (zone1.MinZ + zone1.MaxZ) / 2;
                    int xmid2 = (zone2.MinX + zone2.MaxX) / 2;
                    int zmid2 = (zone2.MinZ + zone2.MaxZ) / 2;
                    int dx = xmid1 - xmid2;
                    int dz = zmid1 - zmid2;
                    float dist = (float)Math.Sqrt(dx * dx + dz * dz);
                    genZone1.AddNearbyZone(zone2, dist);
                    genZone2.AddNearbyZone(zone1, dist);
                }

            }

        }
        _mapGenService.SetPrevNextZones(_gs);
        await Task.CompletedTask;
    }


}



