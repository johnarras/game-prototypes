using OxDb.SharedCore.LineGen;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BaseAddMountains : BaseZoneGenerator
{
    protected ILineGenService _lineGenService = null;

    public override async Awaitable Generate(CancellationToken token)
    {

        await Task.CompletedTask;
    }


    public float GetMountainHeightMult(MyRandom rand)
    {


        float heightMult = 1.0f;
        float totalChance = MapConstants.MountainZeroHeightChance + MapConstants.MountainRandomHeightChance;
        double chosenChance = rand.NextDouble();
        if (chosenChance < MapConstants.MountainZeroHeightChance)
        {
            heightMult = 0.01f;
        }
        else if (chosenChance < totalChance)
        {
            heightMult = RandUtils.FloatRange(0.2f, 1.0f, rand);
        }
        return heightMult;
    }

    protected void AddMountainRidge(int sx, int sz, int ex, int ez, long seed, bool boring, float heightMult, bool secondaryMountain = false)
    {
        if (heightMult < 0.01f)
        {
            heightMult = 0.01f;
        }

        LineGenParameters boringLP = new LineGenParameters();
        boringLP.MaxWidthPosDrift = 0;
        boringLP.MinWidthSize = 1;
        boringLP.MaxWidthSize = 1;
        boringLP.WidthPosShiftChance = 0;
        boringLP.WidthPosShiftSize = 0;
        boringLP.WidthSizeChangeAmount = 0;
        boringLP.WidthSizeChangeChance = 0;
        boringLP.LinePathNoiseScale = 0;
        LineGenParameters lp = new LineGenParameters();

        MyRandom lineRand = new MyRandom(seed);

        int mountainWidth = (int)_md.GetMountainDefaultSize(_mapProvider.GetMap());


        mountainWidth = (int)(RandUtils.IntRange(mountainWidth * 4 / 5, mountainWidth * 6 / 5, lineRand));

        if (secondaryMountain)
        {
            mountainWidth = (int)(mountainWidth * heightMult);
        }

        lp.MaxWidthPosDrift = lineRand.Next() % 100 + 100;
        lp.MinWidthSize = 1;
        lp.MaxWidthSize = 1;

        lp.WidthPosShiftChance = RandUtils.FloatRange(0.12f, 0.23f, lineRand);
        lp.WidthPosShiftChance = 0;
        lp.WidthSize = 1;
        lp.WidthSizeChangeAmount = 0;
        lp.WidthSizeChangeChance = 0.0f;
        lp.LinePathNoiseScale = RandUtils.FloatRange(0.0f, 0.1f, lineRand);
        //lp.LinePathNoiseScale = 0;
        lp.Seed = lineRand.Next();

        int dx = Math.Abs(sx - ex);
        int dz = Math.Abs(sz - ez);


        int maxLen = Math.Max(dx, dz);

        List<LineCell> points = _lineGenService.GetBressenhamLine(new Point2I(sx, sz), new Point2I(ex, ez), (boring ? boringLP : lp));
        if (points == null || points.Count < 1)
        {
            return;
        }
        int startWallWidth = Math.Max(1, lineRand.Next(mountainWidth * 9 / 10, mountainWidth * 11 / 10));

        float baseFreqScaling = MathUtil.Sqrt((sx - ex) * (sx - ex) + (sz - ez) * (sz - ez));

        AddMountainPoints(points, startWallWidth, baseFreqScaling, lineRand.Next(),
            maxLen, secondaryMountain, heightMult);
    }

    public void AddMountainPoints(List<LineCell> points, int startWallWidth, float baseFreqScaling,
        int randSeed, int maxLen, bool secondaryMountain, float heightMult)
    {
        MyRandom lineRand = new MyRandom(randSeed / 3 + 183892);

        float amp = RandUtils.FloatRange(0.07f, 0.22f, lineRand) * 0.8f;
        float freq = RandUtils.FloatRange(0.1f, 0.3f, lineRand) * maxLen * 0.01f * 0.8f;

        int octaves = 2;
        float pers = RandUtils.FloatRange(0.1f, 0.3f, lineRand);
        float[,] mainHeightNoise = _noiseService.Generate(pers, freq, amp, octaves, lineRand.Next(), points.Count, 1);

        float wfreq = RandUtils.FloatRange(0.05f, 0.1f, lineRand) * baseFreqScaling;
        float wamp = RandUtils.FloatRange(0.2f, 0.4f, lineRand);
        float wpers = RandUtils.FloatRange(0.4f, 0.7f, lineRand);
        int woctaves = 2;
        float[,] widthNoise = _noiseService.Generate(wpers, wfreq, wamp, woctaves, lineRand.Next(), points.Count, 1);

        float wallHeightScale = RandUtils.FloatRange(0.8f, 1.05f, lineRand);
        int currWallWidth = Math.Max(1, startWallWidth + lineRand.Next() % 3 - lineRand.Next() % 3);
        for (int l = 0; l < points.Count; l++)
        {
            LineCell item = points[l];
            float mainHeight = mainHeightNoise[l, 0];
            int cx = (int)(item.X);
            int cz = (int)(item.Z);

            currWallWidth = (int)(startWallWidth * (1 + widthNoise[l, 0]));

            if (cx < 0 || cz < 0 || cx >= _mapProvider.GetMap().GetHwid() || cz >= _mapProvider.GetMap().GetHhgt())
            {
                continue;
            }
            if (_md.MapZoneIds[cx, cz] < 1)
            {
                _md.MapZoneIds[cx, cz] = MapConstants.MountainZoneId;
            }
            float heightToSet = (1.1f * (1.0f + mainHeight)) * wallHeightScale * heightMult * MapConstants.MountainHeightMult;
            if (_md.MaintainHeights[cx, cz] < heightToSet)
            {
                _md.MaintainHeights[cx, cz] = heightToSet;
            }

            _md.MountainDistPercent[cx, cz] = 0f;
            if (!secondaryMountain)
            {
                _md.EdgeMountainDistPercent[cx, cz] = 0f;
            }

            float topWidth = 2;
            int mincmidx = Math.Min(_mapProvider.GetMap().GetHwid() / 2, cx);
            int maxcmidx = Math.Max(_mapProvider.GetMap().GetHwid() / 2, cx);
            int mincmidz = Math.Min(_mapProvider.GetMap().GetHhgt() / 2, cz);
            int maxcmidz = Math.Max(_mapProvider.GetMap().GetHhgt() / 2, cz);
            for (int z = Math.Max(0, cz - currWallWidth); z <= Math.Min(_mapProvider.GetMap().GetHhgt() - 1, cz + currWallWidth); z++)
            {
                int ddz = Math.Abs(z - cz);
                for (int x = Math.Max(0, cx - currWallWidth); x <= Math.Min(_mapProvider.GetMap().GetHwid() - 1, cx + currWallWidth); x++)
                {
                    int ddx = Math.Abs(x - cx);

                    double currDist = Math.Sqrt(ddx * ddx + ddz * ddz);
                    double distPct = currDist / currWallWidth;

                    if (currDist < topWidth)
                    {
                        distPct = 0;
                    }
                    else
                    {
                        distPct = (currDist - topWidth) / (currWallWidth - topWidth);
                    }

                    if (distPct >= 1)
                    {
                        continue;
                    }

                    if (_md.MapZoneIds[x, z] == 0)
                    {
                        _md.MapZoneIds[x, z] = MapConstants.MountainZoneId;
                    }
                    if (_md.MountainDistPercent[x, z] > distPct)
                    {
                        _md.MountainDistPercent[x, z] = (float)distPct;
                    }
                    if (_md.MountainCenterDist[x, z] > currDist)
                    {
                        _md.MountainCenterDist[x, z] = (float)(currDist);
                        _md.NearestMountainTopHeight[x, z] = heightToSet;
                    }
                    if (!secondaryMountain)
                    {
                        if (_md.EdgeMountainDistPercent[x, z] > distPct)
                        {
                            _md.EdgeMountainDistPercent[x, z] = (float)distPct;
                        }
                    }
                    _md.Flags[x, z] |= MapGenFlags.IsEdgeWall;
                    float currPower = MathUtil.Clamp(0.5f, 1.7f, 1.0f + _md.MountainDecayPower[x, z]);
                    float newPct = _md.MaintainHeights[cx, cz] * (float)(1.0f - Math.Pow(distPct, currPower));

                    if (newPct != 0 && _md.MaintainHeights[x, z] == 0 && secondaryMountain)
                    {
                        _md.Flags[x, z] |= MapGenFlags.IsSecondaryWall;
                    }

                    if (newPct > _md.MaintainHeights[x, z])
                    {
                        _md.MaintainHeights[x, z] = newPct;
                    }

                }
            }
        }
    }
}

