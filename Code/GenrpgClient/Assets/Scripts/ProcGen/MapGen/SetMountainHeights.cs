
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.ProcGen.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SetMountainHeights : BaseAddMountains
{
    public override async Awaitable Generate(CancellationToken token)
    {


        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed % 1000000000 + 3323292);

        float pers = RandUtils.FloatRange(0.15f, 0.25f, rand);
        int octaves = 2;
        float amp = RandUtils.FloatRange(0.30f, 0.45f, rand);
        float freq = RandUtils.FloatRange(0.04f, 0.09f, rand) * _mapProvider.GetMap().GetHwid();

        float powerpers = RandUtils.FloatRange(0.10f, 0.20f, rand);
        int poweroctaves = 2;
        float poweramp = RandUtils.FloatRange(0.14f, 0.25f, rand);
        float powerfreq = RandUtils.FloatRange(0.06f, 0.10f, rand) * _mapProvider.GetMap().GetHwid();


        float edgepers = RandUtils.FloatRange(0.20f, 0.30f, rand);
        int edgeoctaves = 2;
        float edgeamp = RandUtils.FloatRange(0.40f, 0.70f, rand);
        float edgefreq = RandUtils.FloatRange(0.03f, 0.05f, rand) * _mapProvider.GetMap().GetHwid();


        float edgePowPers = RandUtils.FloatRange(0.20f, 0.30f, rand);
        int edgePowoctaves = 2;
        float edgePowamp = RandUtils.FloatRange(0.30f, 0.50f, rand);
        float edgePowfreq = RandUtils.FloatRange(0.03f, 0.06f, rand) * _mapProvider.GetMap().GetHwid();



        float[,] noise = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());
        float[,] powernoise = _noiseService.Generate(powerpers, powerfreq, poweramp, poweroctaves, rand.Next(), _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());
        float[,] edgeNoise = _noiseService.Generate(edgepers, edgefreq, edgeamp, edgeoctaves, rand.Next(), _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());
        float[,] edgePowNoise = _noiseService.Generate(edgePowPers, edgePowfreq, edgePowamp, edgePowoctaves, rand.Next(), _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());

        float mountainDefaultHeight = _md.GetMountainDefaultSize(_mapProvider.GetMap()) * RandUtils.FloatRange(0.8f, 1.0f, rand);

        float minDistPctCutoff = 0.9f;


        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int z = 0; z < _mapProvider.GetMap().GetHhgt(); z++)
            {

                int ax = Math.Min(x, _mapProvider.GetMap().GetHwid() - 1);
                int az = Math.Min(z, _mapProvider.GetMap().GetHhgt() - 1);


                float noiseScale = 1.0f;
                float whh = _md.MaintainHeights[x, z];

                float roadCheckDistance = MapConstants.MaxRoadCheckDistance;
                float minNoiseDistance = 12.0f;
                float roadDist = _md.RoadDistances[x, z];

                if (_md.RoadDistances[x, z] < roadCheckDistance)
                {
                    float rpct = roadDist / roadCheckDistance;
                    rpct = (float)(Math.Pow(rpct, 1.6f));
                    float edgeDist = MathUtil.Clamp(0.10f, 0.30f + edgeNoise[x, z], 0.70f);
                    float edgeAmt = (float)(Math.Pow(edgeDist, 1.7f + edgePowNoise[x, z]));

                    float currAmt = rpct * rpct;
                    float noiseVal = noise[x, z];
                    float noiseMinDist = MapConstants.RoadBaseHillScaleDistance * (1 + noiseVal);
                    noiseMinDist = MathUtil.Clamp(minNoiseDistance, noiseMinDist, roadCheckDistance);

                    noiseMinDist = 20.0f;
                    if (_md.RoadDistances[x, z] < noiseMinDist)
                    {
                        float currPower = 1.8f;
                        currPower *= MathUtil.Clamp(1.0f, (1.0f + powernoise[x, z]), 2.0f);
                        noiseScale *= (float)(Math.Pow(roadDist / noiseMinDist, currPower));
                    }
                    if (rpct <= edgeDist)
                    {
                        whh *= currAmt;
                    }
                    else if (rpct < 1)
                    {
                        whh *= (edgeAmt + ((rpct - edgeDist) / (1 - edgeDist)) * (1 - edgeAmt));
                    }
                }

                if (_md.MaintainHeights[x, z] == 0 || _md.MountainDistPercent[x, z] >= 1.0f)
                {
                    if (FlagUtils.MatchesAnyBits(_md.Flags[x, z], MapGenFlags.OverrideWallNoiseScale))
                    {
                        _md.Heights[x, z] += _md.MountainNoise[x, z] * noiseScale / MapConstants.MapHeight;
                    }
                    continue;
                }

                float distPct = _md.MountainDistPercent[x, z];
                if (distPct >= minDistPctCutoff && distPct <= 1)
                {
                    float noiseMult = 1 - (distPct - minDistPctCutoff) / (1.0f - minDistPctCutoff);
                    whh *= noiseMult;
                }

                float edgePercent = (float)Math.Pow(_md.EdgeHeightmapAdjustPercent(_mapProvider.GetMap(), x, z), 0.09f);

                whh *= edgePercent;
                if (whh != 0)
                {
                    _md.Heights[x, z] += (mountainDefaultHeight / MapConstants.MapHeight) * whh;
                    _md.ClearAlphasAt(x, z);
                    _md.Alphas[x, z, TerrainTexChannels.Base] = 1.0f;
                }
                float currentNoise = Math.Abs(_md.MountainNoise[x, z]);
                float maxNoise = Math.Abs(_md.MaintainHeights[x, z]) * mountainDefaultHeight * 0.2f;

                if (maxNoise < 0.0001f || currentNoise < 0.0001f)
                {
                    continue;
                }

                if (currentNoise > maxNoise)
                {
                    noiseScale *= (maxNoise) / currentNoise;
                }

                if (FlagUtils.MatchesAnyBits(_md.Flags[x, z], MapGenFlags.OverrideWallNoiseScale))
                {
                    noiseScale = 1.0f;
                }

                _md.Heights[x, z] += _md.MountainNoise[x, z] * noiseScale * edgePercent / MapConstants.MapHeight;
            }
        }
        await Task.CompletedTask;
    }
}

