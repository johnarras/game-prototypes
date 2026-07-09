
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.ProcGen.Entities;
using OxDb.SharedGame.ProcGen.Settings.Locations;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AddRoadDips : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.MinX, zone.MinZ, zone.MaxX, zone.MaxZ);
        }
    }

    public void GenerateOne(Zone zone, ZoneType zoneType, int startx, int startz, int endx, int endz)
    {
        if (zone == null || zoneType == null || startx >= endx || startz >= endz)
        {
            return;
        }

        GenZone genZone = _md.GetGenZone(zone.IdKey);

        float dipScale = genZone.RoadDipScale * zoneType.RoadDipScale;

        int dx = endx - startx + 1;
        int dz = endz - startz + 1;

        float maxSize = Math.Max(dx, dz);
        MyRandom rand = new MyRandom(zone.Seed % 23422423);

        List<float[,]> noises = new List<float[,]>();

        int noiseTimes = 1;

        for (int i = 0; i < noiseTimes; i++)
        {
            float freq = RandUtils.FloatRange(0.03f, 0.10f, rand) * maxSize * 1.5f;
            float amp = RandUtils.FloatRange(0.1f, 0.3f, rand) * 4;
            float pers = RandUtils.FloatRange(0.2f, 0.5f, rand);
            int octaves = 2;

            float[,] noise = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), dx, dz);
            noises.Add(noise);
        }

        int zoneRad = 3;

        int maxDist = RandUtils.IntRange(6, 11, rand);

        dipScale *= 1.0f * maxDist / 6.0f;

        for (int x = startx; x < endx; x++)
        {
            for (int z = startz; z < endz; z++)
            {

                Location loc = _zoneGenService.FindMapLocation(x, z, 5);

                if (loc != null)
                {
                    continue;
                }


                float hx = 1.0f * x / _mapProvider.GetMap().GetHwid();
                float hz = 1.0f * z / _mapProvider.GetMap().GetHhgt();

                float wallDistScale = MathUtil.Clamp(0, _md.MountainDistPercent[x, z], 1);

                if (_md.MapZoneIds[x, z] != zone.IdKey)
                {
                    continue;
                }

                double closestOtherZoneDist = zoneRad;

                for (int xx = x - zoneRad; xx <= x + zoneRad; xx++)
                {
                    if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                    {
                        continue;
                    }

                    for (int zz = z - zoneRad; zz <= z + zoneRad; zz++)
                    {
                        if (zz < 0 || zz >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }
                        if (_md.MapZoneIds[xx, zz] != zone.IdKey)
                        {
                            double dist = Math.Sqrt((x - xx) * (x - xx) + (z - zz) * (z - zz));
                            if (dist < closestOtherZoneDist)
                            {
                                closestOtherZoneDist = dist;
                            }
                        }
                    }
                }

                float distToRoad = _md.RoadDistances[x, z];

                if (distToRoad > maxDist)
                {
                    continue;
                }
                float pct = 1;

                if (distToRoad > 0)
                {
                    pct = MathUtil.Clamp(0, 1 - distToRoad / maxDist, 1);
                }
                if (closestOtherZoneDist < zoneRad)
                {
                    pct *= (float)(Math.Pow(closestOtherZoneDist / zoneRad, 2));
                }

                for (int i = 0; i < noises.Count; i++)
                {
                    pct *= (1.0f + noises[i][x - startx, z - startz]);
                }

                float val = MapConstants.RoadDipHeight * pct * dipScale * wallDistScale;

                _md.Heights[x, z] -= Math.Abs(val);

            }
        }
    }
}



