
using OxDb.SharedCore.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AddZoneNoise : BaseZoneGenerator
{
    private const float _amplitude = 3.9f;
    private const float _persistence = 0.5f;
    private const float _freqDiv = 1000f;
    private const float _lacunarity = 1.5f;

    public override async Awaitable Generate(CancellationToken token)
    {
        int noiseSize = _mapProvider.GetMap().GetHwid();
        float ampDelta = 0.05f;
        float zoneAmp = _amplitude * RandUtils.DeltaScale(ampDelta, _rand.Rand);
        float denomDelta = 0.05f;
        float zoneDenom = _freqDiv * RandUtils.DeltaScale(denomDelta, _rand.Rand);
        float persDelta = 0.05f;
        float pers = _persistence * RandUtils.DeltaScale(persDelta, _rand.Rand);
        float freq = noiseSize / zoneDenom;
        float lacDelta = 0.05f;
        float lac = _lacunarity * RandUtils.DeltaScale(lacDelta, _rand.Rand);

        int seed = _rand.Rand.Next();
        float[,] heights = _noiseService.Generate(pers, noiseSize / zoneDenom, zoneAmp, 2, seed, noiseSize, noiseSize, 0.5f);

        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int y = 0; y < _mapProvider.GetMap().GetHhgt(); y++)
            {
                // Do 1-heights here since most heights are near 0, and few are near 1, we want
                // few near 0 and many near 1 so when the pct is low, very few pieces of
                // terrain will be affected.
                _md.overrideZoneScales[x, y] = 1 - MathUtil.Clamp(0, Math.Abs(heights[x, y]), 1);
            }
        }

        await Task.CompletedTask;
    }
}


