using OxDb.SharedCore.Utils;
using OxDb.SharedGame.ProcGen.Settings.Locations;
using OxDb.SharedGame.ProcGen.Settings.Locations.Constants;
using System;
using System.Threading;
using UnityEngine;

public class AddSecondaryLocations : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        PlaceOtherLocations(_gs);
    }

    private void PlaceOtherLocations(IClientGameState gs)
    {
        long locationsDesired = (int)(_mapProvider.GetMap().BlockCount * _mapProvider.GetMap().BlockCount * 0.05f);

        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed % 1000000000 + 176283);


        if (locationsDesired < 1)
        {
            locationsDesired = 1;
        }
        locationsDesired = Math.Max(1, locationsDesired / 2 + rand.Next() % (locationsDesired + 1));

        int edgeDistance = 2 * MapConstants.TerrainPatchSize;

        if (_mapProvider.GetMap().GetHwid() <= edgeDistance * 2 || _mapProvider.GetMap().GetHhgt() <= edgeDistance * 2)
        {
            return;
        }

        int minDistToFeature = 40;

        long locationsPlaced = 0;

        int mountainCheckRadius = 50;

        for (int times = 0; times < locationsDesired * 100 && locationsPlaced < locationsDesired; times++)
        {
            int cx = edgeDistance + rand.Next() % (_mapProvider.GetMap().GetHwid() - 2 * edgeDistance);
            int cz = edgeDistance + rand.Next() % (_mapProvider.GetMap().GetHhgt() - 2 * edgeDistance);

            // Not near current loc.
            Location nearLoc = _zoneGenService.FindMapLocation(cx, cz, minDistToFeature);
            if (nearLoc != null)
            {
                continue;
            }

            bool failed = false;
            for (int xx = cx - mountainCheckRadius; xx <= cx + mountainCheckRadius; xx++)
            {
                if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                {
                    continue;
                }

                for (int zz = cz - mountainCheckRadius; zz <= cz + mountainCheckRadius; zz++)
                {
                    if (zz < 0 || zz >= _mapProvider.GetMap().GetHhgt())
                    {
                        continue;
                    }

                    if (FlagUtils.MatchesAnyBits(base._md.Flags[xx, zz], MapGenFlags.IsEdgeWall))
                    {
                        failed = true;
                    }
                }
            }

            if (failed)
            {
                continue;
            }

            if (base._md.RoadDistances[cx, cz] < minDistToFeature)
            {
                continue;
            }

            if (base._md.MapZoneIds[cx, cz] < MapConstants.MapZoneStartId)
            {
                continue;
            }

            int minRad = 5;
            int maxRad = 10;

            if (rand.NextDouble() < 0.2f)
            {
                minRad *= 2;
                maxRad *= 2;
            }

            Location loc = new Location()
            {
                CenterX = cx,
                CenterZ = cz,
                LocationTypeId = LocationTypes.Secondary,
                XSize = RandUtils.IntRange(minRad, maxRad, rand),
                ZSize = RandUtils.IntRange(minRad, maxRad, rand),
            };

            base._md.AddMapLocation(_mapProvider, loc);
            locationsPlaced++;
        }
    }


}



