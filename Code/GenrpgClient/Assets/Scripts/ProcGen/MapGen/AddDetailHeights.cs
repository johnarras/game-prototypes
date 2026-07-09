
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.ProcGen.Entities;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Threading;
using UnityEngine;

public class AddDetailHeights : BaseZoneGenerator
{

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        foreach (Zone zn in _mapProvider.GetMap().Zones)
        {
            GenerateOneZone(zn, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zn.ZoneTypeId), zn.MinX, zn.MinZ, zn.MaxX, zn.MaxZ);
        }
    }

    public void GenerateOneZone(Zone zone, ZoneType zoneType, int startx, int startz, int endx, int endz)
    {
        if (zone == null || zoneType == null)
        {
            return;
        }

        if (startx < 0)
        {
            startx = 0;
        }

        if (startz < 0)
        {
            startz = 0;
        }

        if (endx >= _mapProvider.GetMap().GetHwid())
        {
            endx = _mapProvider.GetMap().GetHwid() - 1;
        }

        if (endz >= _mapProvider.GetMap().GetHhgt())
        {
            endz = _mapProvider.GetMap().GetHhgt() - 1;
        }

        if (endx <= startx || endz <= startz)
        {
            return;
        }

        if (_zoneGenService == null)
        {
            return;
        }

        int wid = endx - startx;
        int hgt = endz - startz;

        int awid = _md.Awid;
        int ahgt = _md.Ahgt;


        MyRandom rand = new MyRandom(zone.Seed + _mapProvider.GetMap().Seed / 7);

        GenZone genZone = _md.GetGenZone(zone.IdKey);
        float hillAmplitudeScale = 1.0f;
        hillAmplitudeScale *= genZone.DetailAmp;
        hillAmplitudeScale *= zoneType.DetailAmp;
        if (hillAmplitudeScale < 0.1f)
        {
            hillAmplitudeScale = 0.1f;
        }

        float hillFrequencyScale = 1.0f;
        hillFrequencyScale *= genZone.DetailFreq;
        hillFrequencyScale *= zoneType.DetailFreq;
        if (hillFrequencyScale < 0.1f)
        {
            hillFrequencyScale = 0.1f;
        }

        float perlinScale = 1.0f;

        int perlinSize = MapConstants.DefaultNoiseSize;

        int maxSize = Math.Max(wid, hgt);
        if (maxSize > MapConstants.DefaultNoiseSize)
        {
            perlinScale = 1.0f * maxSize / MapConstants.DefaultNoiseSize;
            perlinSize = maxSize;
        }

        float startFreq = perlinSize * hillFrequencyScale * 0.009f;
        // Amplitude for details increases a bit as the size of the zone increases.
        float startAmp = 0.0150f * (float)(Math.Pow(perlinScale, 0.2f));

        float ampDelta = 0.35f;
        float freqDelta = 0.35f;

        float pers = 0.40f; float amp = startAmp; float freq = startFreq; int octaves = 2;

        float startExp = 0.0f;

        long pseed = zone.Seed + 34543;
        freq = startFreq * RandUtils.DeltaScale(freqDelta, rand);
        amp = startAmp * RandUtils.DeltaScale(ampDelta, rand);

        float extraScale = 1.0f;

        extraScale = RandUtils.FloatRange(0.9f, 1.3f, rand);
        freq /= extraScale;
        amp *= extraScale;

        float exp = startExp * RandUtils.FloatRange(0.5f, 1.5f, rand);

        amp *= 0.95f;
        pers *= 1.05f;

        float[,] heightsUp = _noiseService.Generate(pers, freq, amp, octaves, pseed, perlinSize, perlinSize, exp);

        freq = startFreq * RandUtils.DeltaScale(freqDelta, rand) * 0.45f;
        amp = startAmp * RandUtils.DeltaScale(ampDelta, rand) * 1.45f;
        exp = startExp * RandUtils.FloatRange(0.5f, 1.5f, rand);

        extraScale = RandUtils.FloatRange(0.9f, 1.3f, rand);
        freq /= extraScale;
        amp *= extraScale;

        float[,] heightsUp2 = _noiseService.Generate(pers, freq, amp, octaves, pseed / 6 + 21412, perlinSize, perlinSize, exp);

        freq = startFreq * RandUtils.DeltaScale(freqDelta, rand) * 0.9f;
        amp = startAmp * RandUtils.DeltaScale(ampDelta, rand) * 1.1f;
        exp = startExp * RandUtils.FloatRange(0.5f, 1.5f, rand);

        extraScale = RandUtils.FloatRange(0.9f, 1.3f, rand);
        freq /= extraScale;
        amp *= extraScale;

        float[,] heightsDown = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), perlinSize, perlinSize, exp);

        float effPers = RandUtils.FloatRange(0.05f, 0.2f, rand);
        float effFreq = RandUtils.FloatRange(0.05f, 0.10f, rand) * perlinSize;
        float effAmp = RandUtils.FloatRange(0.04f, 0.12f, rand);

        float[,] roadEffectPercent = _noiseService.Generate(effPers, effFreq, effAmp, 2, rand.Next(), perlinSize, perlinSize);

        float[,,] alphamaps = _md.Alphas;

        float roadAffectedPercent = zoneType.RoadDetailScale * 0.15f;

        float detailMult = 1.0f;

        float startRad = 40;

        float radPers = RandUtils.FloatRange(0.1f, 0.4f, rand);
        float radFreq = RandUtils.FloatRange(0.02f, 0.1f, rand) * perlinSize;
        float radAmp = RandUtils.FloatRange(0.3f, 0.6f, rand) * startRad;

        float[,] radValues = _noiseService.Generate(radPers, radFreq, radAmp, 2, rand.Next(), perlinSize, perlinSize);

        float startPower = 1.5f;

        float powerPers = RandUtils.FloatRange(0.1f, 0.4f, rand);
        float powerFreq = RandUtils.FloatRange(0.02f, 0.1f, rand) * perlinSize;
        float powerAmp = RandUtils.FloatRange(0.3f, 0.6f, rand);

        float[,] powerValues = _noiseService.Generate(powerPers, powerFreq, powerAmp, 2, rand.Next(), perlinSize, perlinSize);

        int numTries = 0;

        for (int x = 0; x < wid; x++)
        {
            for (int z = 0; z < hgt; z++)
            {

                if (heightsUp[x, z] < 0)
                {
                    heightsUp[x, z] /= 4;
                }

                if (heightsDown[x, z] < 0)
                {
                    heightsDown[x, z] = 0;
                }

                if (heightsUp2[x, z] < 0)
                {
                    heightsUp2[x, z] /= 4;
                }

                heightsUp2[x, z] *= detailMult;

                int wx = x + startx;
                int wz = z + startz;

                float roadHeightMult = 1.0f;


                if (false && alphamaps[wx, wz, TerrainTexChannels.Road] > 0.12f)
                {
                    roadHeightMult = 0;
                }
                else
                {
                    float roadDist = _md.RoadDistances[wx, wz];



                    float rad = MathUtil.Clamp(startRad / 2, startRad + radValues[x, z], MapConstants.MaxRoadCheckDistance);

                    if (roadDist < rad)
                    {
                        float scaleDown = roadDist / rad;
                        float currPower = MathUtil.Clamp(1.0f, startPower + powerValues[x, z], 2.0f);
                        roadHeightMult *= (float)(Math.Pow(scaleDown, currPower));


                    }
                }
                if (roadHeightMult > 1)
                {
                    roadHeightMult = 1;
                }

                if (roadHeightMult < roadAffectedPercent)
                {
                    if (roadHeightMult <= 0)
                    {
                        roadHeightMult = roadAffectedPercent / 5;
                    }
                    else
                    {
                        roadHeightMult = roadAffectedPercent;
                    }
                }

                float edgeSmoothMult = 1.0f;

                int rad2 = 15;
                int totalNum = 0;
                int numOtherNearby = 0;
                if (wx >= rad2 && wx < _mapProvider.GetMap().GetHwid() - rad2 - 1 && wz >= rad2 && wz <= _mapProvider.GetMap().GetHhgt() - rad2 - 1)
                {
                    double minDist = 100000;
                    for (int xx = wx - rad2; xx <= wx + rad2; xx++)
                    {
                        for (int zz = wz - rad2; zz <= wz + rad2; zz++)
                        {
                            totalNum++;
                            if (_md.MapZoneIds[xx, zz] != zone.IdKey)
                            {
                                numOtherNearby++;
                                double newDist = Math.Sqrt((xx - wx) * (xx - wx) + (zz - wz) * (zz - wz));
                                if (newDist < minDist)
                                {
                                    minDist = newDist;
                                }
                            }
                        }
                    }

                    if (minDist <= 0)
                    {
                        edgeSmoothMult = 0;
                    }
                    else if (minDist < rad2)
                    {
                        edgeSmoothMult = (float)(Math.Pow(minDist / rad2, 1.5f));
                    }
                }

                float downHeight = heightsDown[x, z];
                numTries++;


                float heightDiff = hillAmplitudeScale * (heightsUp[x, z] + heightsUp2[x, z] - downHeight);

                // The idea here is we scale down mountains and valleys if a road is there,
                // but the max scaledown is currently to 20% of the actual height.
                // But if the heightdiff for this detail is <= 0.02, we scale down this diff
                // so we look at Math.min(actualHeightdiff/0.02,1) to get how much we should really
                // scale down, and add the rest of the part back into the height mult.


                float worldEdgePercent = (float)Math.Pow(_md.EdgeHeightmapAdjustPercent(_mapProvider.GetMap(), wx, wz), 0.09f);


                float finalHeightDiff = heightDiff * roadHeightMult * edgeSmoothMult * worldEdgePercent;

                _md.Heights[wx, wz] += finalHeightDiff;
            }
        }

    }



}



