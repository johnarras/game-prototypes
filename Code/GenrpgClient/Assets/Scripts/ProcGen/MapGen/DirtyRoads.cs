using OxDb.SharedCore.Utils;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Threading;
using UnityEngine;

public class DirtyRoads : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.MinX, zone.MaxX, zone.MinZ, zone.MaxZ);
        }

    }

    public void GenerateOne(Zone zone, ZoneType zoneType, int minx, int maxx, int minz, int maxz)
    {
        if (zone == null)
        {
            return;
        }

        int sizex = maxx - minx + 1;
        int sizez = maxz - minz + 1;

        if (sizex < 10 || sizez < 10)
        {
            return;
        }

        int size = Math.Max(sizex, sizez);

        MyRandom rand = new MyRandom(zone.Seed + 724334);

        float globalScale = 1.25f;

        float amp = RandUtils.FloatRange(0.6f, 1.3f, rand) * globalScale;
        float freq = RandUtils.FloatRange(0.2f, 0.3f, rand) * size * globalScale;
        float pers = RandUtils.FloatRange(0.4f, 0.7f, rand) * globalScale;
        int octaves = 2;

        float[,] dirtHeights = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), sizex, sizez);

        float minRoadPercent = 0.20f;

        float maxOtherPercent = 0.80f;

        amp = RandUtils.FloatRange(0.6f, 1.3f, rand) * globalScale;
        freq = RandUtils.FloatRange(0.15f, 0.25f, rand) * size * globalScale;
        pers = RandUtils.FloatRange(0.4f, 0.7f, rand) * globalScale;
        octaves = 2;
        float[,] baseHeights = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), sizex, sizez);


        float startMaxPct = 0.85f;

        float pctamp = RandUtils.FloatRange(0.2f, 0.3f, rand) * globalScale;
        float pctfreq = RandUtils.FloatRange(0.1f, 0.2f, rand) * size * globalScale;
        float pctpers = RandUtils.FloatRange(0.0f, 0.4f, rand) * globalScale;
        int pctoctaves = 2;
        float[,] maxPcts = _noiseService.Generate(pctpers, pctfreq, pctamp, pctoctaves, rand.Next(), sizex, sizez);


        float generalPerturb = 0.1f;

        for (int x = minx; x <= maxx; x++)
        {
            if (x < 0 || x >= _mapProvider.GetMap().GetHwid())
            {
                continue;
            }

            for (int z = minz; z <= maxz; z++)
            {
                if (z < 0 || z >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }

                if (_md.Alphas[x, z, TerrainTexChannels.Road] < minRoadPercent)
                {
                    continue;
                }

                if (_md.MapZoneIds[x, z] != zone.IdKey)
                {
                    continue;
                }

                // Get height > 0
                //float dirtPct = Math.Abs (dirtHeights[x,z]);
                //float basePct = Math.Abs (baseHeights[x,z]);
                float dirtPct = MathUtil.Clamp(0, dirtHeights[x - minx, z - minz] + RandUtils.DeltaRange(generalPerturb, rand), maxOtherPercent);
                float basePct = MathUtil.Clamp(0, baseHeights[x - minx, z - minz] + RandUtils.DeltaRange(generalPerturb, rand), maxOtherPercent);

                float totalPct = dirtPct + basePct;


                float maxPct = maxPcts[x - minx, z - minz] + startMaxPct;

                if (totalPct > maxPct)
                {
                    dirtPct /= (totalPct / maxPct);
                    basePct /= (totalPct / maxPct);
                    totalPct = maxPct;
                }

                _md.ClearAlphasAt(x, z);
                _md.Alphas[x, z, TerrainTexChannels.Road] = 1 - totalPct;
                _md.Alphas[x, z, TerrainTexChannels.Dirt] = dirtPct;
                _md.Alphas[x, z, TerrainTexChannels.Base] = basePct;

            }
        }
    }
}

