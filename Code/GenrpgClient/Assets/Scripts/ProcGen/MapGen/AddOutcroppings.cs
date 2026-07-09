using OxDb.SharedCore.LineGen;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AddOutcroppings : BaseZoneGenerator
{
    const int MinSize = 60;
    const int MaxSize = 400;
    const int GridBuffer = 200;
    const int GridSize = MaxSize + 2 * GridBuffer;

    const int MaxGridIndex = 2;
    float[,,] grids = null;

    protected ILineGenService _lineGenService = null;

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        grids = new float[GridSize, GridSize, MaxGridIndex];
        ClearGrid();

        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.MinX, zone.MinZ, zone.MaxX, zone.MaxZ);
        }
    }

    private void ClearGrid()
    {
        for (int i = 0; i < 2; i++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int z = 0; z < GridSize; z++)
                {
                    grids[x, z, i] = 0;
                }
            }
        }
    }

    public void GenerateOne(Zone zone, ZoneType zoneType, int startx, int startz, int endx, int endz)
    {
        if (zone == null || endx <= startx || endz <= startz)
        {
            return;
        }

        MyRandom rand = new MyRandom(zone.Seed % 2000000000 + 15434454);

        int edgeSize = MapConstants.TerrainPatchSize * 2;

        startx = MathUtil.Clamp(edgeSize, startx, _mapProvider.GetMap().GetHwid() - edgeSize);
        endx = MathUtil.Clamp(edgeSize, endx, _mapProvider.GetMap().GetHwid() - edgeSize);

        startz = MathUtil.Clamp(edgeSize, startz, _mapProvider.GetMap().GetHwid() - edgeSize);
        endz = MathUtil.Clamp(edgeSize, endz, _mapProvider.GetMap().GetHwid() - edgeSize);


        int numOutcroppings = 2;


        for (int times = 0; times < 10; times++)
        {
            if (rand.NextDouble() < 0.4f)
            {
                numOutcroppings++;
            }
            else
            {
                break;
            }
        }


        numOutcroppings = 1;

        float InGridVal = 1.0f;
        for (int times = 0; times < numOutcroppings; times++)
        {
            ClearGrid();


            float amp = RandUtils.FloatRange(0.1f, 0.2f, rand);
            float freq = RandUtils.FloatRange(5f, 15f, rand);
            float pers = RandUtils.FloatRange(0.2f, 0.5f, rand);

            float[,] heightScales = _noiseService.Generate(pers, freq, amp, 2, rand.Next(), GridSize, GridSize);

            for (int tries = 0; tries < 20; tries++)
            {
                int sx = RandUtils.IntRange(startx, endx, rand);
                int sz = RandUtils.IntRange(startz, endz, rand);

                int ex = RandUtils.IntRange(startx, endx, rand);
                int ez = RandUtils.IntRange(startz, endz, rand);

                int dx = Math.Abs(ex - sx);
                int dz = Math.Abs(ez - sz);

                if (dx < MinSize || dx > MaxSize || dz < MinSize || dz > MaxSize)
                {
                    continue;
                }
                float finalHeightScale = 1.0f;

                if (rand.NextDouble() < 0.5)
                {
                    finalHeightScale = -finalHeightScale;
                }

                int minSize = Math.Min(dx, dz);
                int maxSize = Math.Max(dx, dz);

                int maxWidth = RandUtils.IntRange(minSize * 2 / 3, minSize, rand);

                float fullHeight = RandUtils.FloatRange(20.0f, 60.0f, rand) / MapConstants.MapHeight;

                int mx = (sx + ex) / 2;
                int mz = (sz + ez) / 2;

                LineGenParameters lineParams = new LineGenParameters()
                {
                    UseOvalWidth = true,
                    MinWidthSize = Math.Max(4, minSize / 10),
                    MaxWidthSize = maxWidth,
                    WidthSizeChangeAmount = maxWidth / 4,
                    WidthSizeChangeChance = 0.2f,
                    Seed = rand.Next(),
                    WidthPosShiftChance = 0.4f,
                    WidthPosShiftSize = 2,
                    LinePathNoiseScale = 1.0f,
                };

                Point2I start = new Point2I(sx, sz);
                Point2I end = new Point2I(ex, ez);

                List<LineCell> line = _lineGenService.GetBressenhamLine(start, end, lineParams);

                if (line.Count < 1)
                {
                    continue;
                }

                int numCenters = 0;
                int numAdjusted = 0;
                if (line != null)
                {
                    foreach (LineCell pt in line)
                    {
                        int px = (int)(pt.X - mx) + GridSize / 2;
                        int pz = (int)(pt.Z - mz) + GridSize / 2;


                        if (px < 1 || pz < 1 || px >= GridSize - 1 || pz >= GridSize - 1)
                        {
                            continue;
                        }
                        grids[px, pz, 0] = InGridVal;
                        numCenters++;

                    }
                }

                List<Point2I> lowestPoints = new List<Point2I>();


                float smoothFreq = RandUtils.FloatRange(0.03f, 0.7f, rand);
                float smoothAmp = RandUtils.FloatRange(0.2f, 0.3f, rand);
                float smoothPers = RandUtils.FloatRange(0.1f, 0.3f, rand);
                int smoothOctaves = 2;

                float[,] smoothNoise = _noiseService.Generate(pers, freq, amp, smoothOctaves, rand.Next(), GridSize, GridSize);


                int baseSmoothRad = Math.Max(7, (int)(fullHeight / 6));


                List<Point2I> potentialLowestPoints = new List<Point2I>();

                for (int x = 0; x < GridSize; x++)
                {
                    for (int z = 0; z < GridSize; z++)
                    {
                        int numCells = 0;
                        float totalSum = 0;
                        int smoothRad = Math.Max(2, (int)(baseSmoothRad * (1 + smoothNoise[x, z])));
                        for (int xx = x - smoothRad; xx <= x + smoothRad; xx++)
                        {
                            if (xx < 0 || xx >= GridSize)
                            {
                                continue;
                            }

                            for (int zz = z - smoothRad; zz <= z + smoothRad; zz++)
                            {
                                if (zz < 0 || zz >= GridSize)
                                {
                                    continue;
                                }

                                totalSum += grids[xx, zz, 0];
                                numCells++;
                            }
                        }

                        if (numCells > 0)
                        {
                            grids[x, z, 1] = totalSum / numCells;
                            if (totalSum < numCells && totalSum > 0)
                            {
                                potentialLowestPoints.Add(new Point2I(x, z));
                            }
                        }
                        else
                        {
                            grids[x, z, 1] = 0;
                        }

                    }
                }
                int numLowestPoints = 1;
                for (int i = 0; i < 3; i++)
                {
                    if (rand.NextDouble() < 0.1 / (i + 1))
                    {
                        numLowestPoints++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (potentialLowestPoints.Count > 10)
                {
                    for (int i = 0; i < numLowestPoints; i++)
                    {
                        lowestPoints.Add(potentialLowestPoints[rand.Next() % potentialLowestPoints.Count]);
                    }
                }

                foreach (Point2I pt in lowestPoints)
                {
                    float lowPointRadius = RandUtils.FloatRange(1.25f, 2.0f, rand) * fullHeight * MapConstants.MapHeight;

                    float power = RandUtils.FloatRange(1.0f, 1.5f, rand);

                    for (int x = 0; x < GridSize; x++)
                    {
                        float ddx = x - pt.X;
                        for (int z = 0; z < GridSize; z++)
                        {
                            float ddz = z - pt.Z;

                            double dist = Math.Sqrt(ddx * ddx + ddz * ddz);
                            float scaleDist = (float)Math.Pow(Math.Min(1.0f, dist / lowPointRadius), power);
                            grids[x, z, 1] *= scaleDist;
                        }
                    }
                }

                // Find lowest point where the grid is >= 1.

                float lowestMapHeight = 1.0f;
                float highestMapHeight = 0.0f;
                for (int x = 0; x < GridSize; x++)
                {
                    int wx = x + mx - GridSize / 2;
                    if (wx < 0 || wx >= _mapProvider.GetMap().GetHwid())
                    {
                        continue;
                    }

                    for (int z = 0; z < GridSize; z++)
                    {
                        int wz = z + mz - GridSize / 2;
                        if (wz < 0 || wz >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }

                        if (grids[x, z, 1] != 0)
                        {
                            float roadDist = _md.RoadDistances[wx, wz];

                            float roadScalePercent = MathUtil.GetSmoothScalePercent(10, 60, roadDist);

                            if (roadDist < 5)
                            {
                                roadScalePercent = 0;
                            }

                            grids[x, z, 1] *= roadScalePercent;

                            float mountainHeight = _md.MountainDistPercent[wx, wz];

                            grids[x, z, 1] *= mountainHeight;

                            if (grids[x, z, 1] == 1)
                            {
                                float hgt = _md.Heights[wx, wz];
                                if (hgt < lowestMapHeight)
                                {
                                    lowestMapHeight = hgt;
                                }

                                if (hgt > highestMapHeight)
                                {
                                    highestMapHeight = hgt;
                                }
                            }
                        }
                    }
                }
                // We target the grid to go to the highest worldheight modified by the noise/scale down.
                if (highestMapHeight <= 0 || lowestMapHeight >= 1)
                {
                    continue;
                }

                float heightDiff = highestMapHeight - lowestMapHeight;
                fullHeight -= heightDiff;
                if (fullHeight < 2.0f / MapConstants.MapHeight)
                {
                    fullHeight = 2.0f / MapConstants.MapHeight;
                }

                for (int x = 0; x < GridSize; x++)
                {
                    int wx = x + mx - GridSize / 2;
                    if (wx < 0 || wx >= _mapProvider.GetMap().GetHwid())
                    {
                        continue;
                    }

                    for (int z = 0; z < GridSize; z++)
                    {
                        int wz = z + mz - GridSize / 2;
                        if (wz < 0 || wz >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }

                        if (grids[x, z, 1] <= 0)
                        {
                            continue;
                        }

                        if (finalHeightScale > 0)
                        {
                            // Calculate the height difference.
                            // grid val * ((maxHeight-worldHeight)+(outcropping overallheight*(1+noise)))
                            float gridval = grids[x, z, 1] * ((highestMapHeight - _md.Heights[wx, wz]) + fullHeight * (1 + heightScales[x, z]));

                            _md.Heights[wx, wz] += gridval * finalHeightScale;
                        }
                        else
                        {
                            float gridval = grids[x, z, 1] * ((_md.Heights[wx, wz] - lowestMapHeight) - fullHeight * (1 + heightScales[x, z]));

                            _md.Heights[wx, wz] += gridval * -finalHeightScale;
                        }
                        numAdjusted++;
                    }
                }

                break;
            }
        }
    }
}

