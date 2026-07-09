using OxDb.SharedCore.Utils;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.ProcGen.Settings.Locations;
using OxDb.SharedGame.ProcGen.Settings.Locations.Constants;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// Connect these zone centers to "closest object.

public class ConnectSecondaryLocations : BaseZoneGenerator
{
    private IAddRoadService _addRoadService = null;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        // This can happen if there are no secondary locations added.
        if (_md.LocationGrid == null)
        {
            return;
        }

        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed / 2 + 9977747);

        List<Location> locations = new List<Location>();

        for (int x = 0; x < _md.LocationGrid.GetLength(0); x++)
        {
            for (int z = 0; z < _md.LocationGrid.GetLength(1); z++)
            {
                if (_md.LocationGrid[x, z] == null)
                {
                    continue;
                }
                foreach (Location loc in _md.LocationGrid[x, z])
                {
                    if (loc.LocationTypeId != LocationTypes.ZoneCenter)
                    {
                        locations.Add(loc);
                    }
                }
            }
        }

        while (locations.Count > 0)
        {
            int pos = rand.Next() % locations.Count;
            Location loc = locations[pos];
            locations.RemoveAt(pos);

            int radiusStart = 10;
            int radiusEnd = 300;
            int rskip = 2;

            int roadx = -1;
            int roadz = -1;
            double minRoadDist = 1000000;

            int cx = loc.CenterX;
            int cz = loc.CenterZ;
            for (int r = radiusStart; r <= radiusEnd; r += rskip)
            {
                if (roadx >= 0 && roadz >= 0 && r > minRoadDist * 5 / 4)
                {
                    break;
                }
                int rad = r / 2;
                int[] zvals = new int[] { cz - rad, cz + rad };

                foreach (int z in zvals)
                {
                    if (z >= 0 && z < _md.Ahgt)
                    {
                        int dz = z - cz;
                        for (int x = cx - rad; x <= cx + rad; x += rskip)
                        {
                            if (x < 0 || x >= _md.Awid)
                            {
                                continue;
                            }

                            if (_md.Alphas[x, z, TerrainTexChannels.Road] > 0)
                            {
                                int dx = x - cx;
                                double dist = Math.Sqrt(dx * dx + dz * dz);
                                if (dist < minRoadDist)
                                {
                                    minRoadDist = dist;
                                    roadx = x;
                                    roadz = z;
                                }
                            }
                        }
                    }
                }

                int[] xvals = new int[] { cx - rad, cx + rad };

                foreach (int x in xvals)
                {
                    if (x >= 0 && x < _md.Awid)
                    {
                        int dx = x - cx;
                        for (int z = cz - rad; z <= cz + rad; z += rskip)
                        {
                            if (z < 0 || z >= _md.Ahgt)
                            {
                                continue;
                            }

                            if (_md.Alphas[x, z, TerrainTexChannels.Road] > 0)
                            {
                                int dz = z - cz;
                                double dist = Math.Sqrt(dx * dx + dz * dz);
                                if (dist < minRoadDist)
                                {
                                    minRoadDist = dist;
                                    roadx = x;
                                    roadz = z;
                                }
                            }
                        }
                    }
                }
            }

            if (roadx > 0 && roadz > 0)
            {
                _addRoadService.AddRoad(cx, cz, roadx, roadz, cx * 31 + cz * 37 + _mapProvider.GetMap().Seed / 3, rand, false);
            }

        }


    }
}






