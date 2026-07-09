using OxDb.SharedCore.LineGen;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CreviceData
{
    public int xStart;
    public int zStart;
    public int xEnd;
    public int zEnd;
    public int xSize;
    public int zSize;
    public Zone zone;
}
public class AddCrevices : BaseZoneGenerator
{

    public const float MinSteepness = 15f;
    public const float MaxSteepness = 60f;
    public const float MaxSteepnessPerturbDelta = 15f;

    public const int SmoothRadiusDelta = 5;
    public const int SmoothRadiusDefault = 12;

    public const int RoadEFfectDist = 16;
    public const int RoadZeroDist = 8;

    protected ILineGenService _lineGenService = null;

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.MinX, zone.MaxX, zone.MinZ, zone.MaxZ);
        }


        SetCreviceDepths(_gs);
        _md.CreviceDepths = null;
    }

    private void SetCreviceDepths(IClientGameState gs)
    {
        if (base._md.Heights == null || base._md.CreviceDepths == null)
        {
            return;
        }

        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int z = 0; z < _mapProvider.GetMap().GetHhgt(); z++)
            {
                float lowerValue = base._md.CreviceDepths[x, z] * MapConstants.DefaultCreviceDepth / MapConstants.MapHeight;

                float roadDist = base._md.RoadDistances[x, z];
                if (roadDist < RoadEFfectDist)
                {
                    if (roadDist < RoadZeroDist)
                    {
                        lowerValue = 0;
                    }
                    else
                    {
                        lowerValue *= 1.0f * (roadDist - RoadZeroDist) / (RoadEFfectDist - RoadZeroDist);
                    }

                }


                base._md.Heights[x, z] += lowerValue;
            }
        }
    }
    public void GenerateOne(Zone zone, ZoneType zoneType, int startx, int endx, int startz, int endz)
    {
        if (zone == null || zoneType == null || startx >= endx || startz >= endz)
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

        int xsize = endx - startx + 1;
        int zsize = endz - startz + 1;

        if (xsize < 1 || zsize < 1)
        {
            return;
        }

        CreviceData cdata = new CreviceData()
        {
            xSize = xsize,
            zSize = zsize,
            xStart = startx,
            zStart = startz,
            xEnd = endx,
            zEnd = endz,
            zone = zone,
        };
        if (_md.CreviceDepths == null)
        {
            _md.CreviceDepths = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
        }

        MyRandom rand = new MyRandom(zone.Seed + 2333);


        int perlinSize = Math.Max(cdata.xSize + 40, cdata.zSize + 40);

        float depthFreq = 5.0f * RandUtils.FloatRange(0.8f, 1.2f, rand);
        float depthAmp = 0.05f * RandUtils.FloatRange(0.8f, 1.2f, rand);
        int depthOctaves = 2;
        float depthPer = 0.3f * RandUtils.FloatRange(0.8f, 1.2f, rand);

        float[,] depthOffsets = _noiseService.Generate(depthPer, depthFreq, depthAmp, depthOctaves, rand.Next() + 1234, perlinSize, perlinSize);

        float smoothFreq = 5.0f * RandUtils.FloatRange(0.8f, 1.2f, rand);
        float smoothAmp = 0.1f * RandUtils.FloatRange(0.8f, 1.2f, rand);
        int smoothOctaves = 2;
        float smoothPer = 0.3f * RandUtils.FloatRange(0.8f, 1.2f, rand);

        float[,] smoothOffsets = _noiseService.Generate(smoothPer, smoothFreq, smoothAmp, smoothOctaves, rand.Next() % 2345, perlinSize, perlinSize);


        float minCrevices = 2.0f;
        float maxCrevices = 6.0f;

        if (zoneType.CreviceCountScale > 0)
        {
            minCrevices *= zoneType.CreviceCountScale;
            maxCrevices *= zoneType.CreviceCountScale;
        }

        int numCrevices = (int)(RandUtils.FloatRange(minCrevices, maxCrevices, rand));


        for (int c = 0; c < numCrevices; c++)
        {
            AddCreviceDepths(cdata, c, zone, zoneType, depthOffsets, smoothOffsets);
        }


    }

    public void AddCreviceDepths(CreviceData cdata, int index, Zone zone, ZoneType zoneType, float[,] depthOffsets, float[,] smoothOffsets)
    {
        if (cdata == null || zone == null || zoneType == null)
        {
            return;
        }

        MyRandom endPtRand = new MyRandom(zone.Seed % 134634657 + 6623 + index * 13 + index * index * 7);

        int sx = 0;
        int sz = 0;
        int ex = 0;
        int ez = 0;

        int edgeSize = 10;

        int size = Math.Max(cdata.xSize, cdata.zSize);
        int times = 0;
        int minSize = size / 30;
        int maxSize = size * 3 / 4;

        edgeSize = 10;
        minSize = size / 3;
        maxSize = size - edgeSize;

        while (++times < 100)
        {

            sx = RandUtils.IntRange(edgeSize, cdata.xSize - edgeSize, endPtRand) + cdata.xStart;
            sz = RandUtils.IntRange(edgeSize, cdata.zSize - edgeSize, endPtRand) + cdata.zStart;
            ex = RandUtils.IntRange(edgeSize, cdata.xSize - edgeSize, endPtRand) + cdata.xStart;
            ez = RandUtils.IntRange(edgeSize, cdata.zSize - edgeSize, endPtRand) + cdata.zStart;

            int dx = Math.Abs(sx - ex);
            int dz = Math.Abs(sz - ez);

            if (endPtRand.NextDouble() < 0.3f)
            {
                sx = (sx + ex) / 2;
                sz = (sz + ez) / 2;
            }
            else if (endPtRand.NextDouble() < 0.3)
            {
                ex = (sx + ex) / 2;
                ez = (sz + ez) / 2;

            }
            if ((dx >= minSize || dz >= minSize) &&
                (dx < maxSize || dz < maxSize))
            {
                break;
            }

        }

        Point2I sp = new Point2I(sx, sz);
        Point2I ep = new Point2I(ex, ez);

        InnerAddCreviceDepths(cdata, zone, zoneType, sp, ep, endPtRand.Next() % 100000000, depthOffsets, smoothOffsets);

    }

    private void InnerAddCreviceDepths(CreviceData cdata, Zone zone, ZoneType zoneType, Point2I sp, Point2I ep, int randSeed,
        float[,] depthOffsets, float[,] smoothOffsets)
    {
        MyRandom crossRand = new MyRandom(zone.Seed % 1000000000 + 662423 + randSeed);
        MyRandom rand = new MyRandom(zone.Seed % 2010102933 + 783124 + randSeed);

        if (cdata == null || _md.CreviceDepths == null)
        {
            return;
        }
        if (_md.CreviceBridges == null)
        {
            _md.CreviceBridges = new List<Point2I>();
        }

        float overallDepthMult = (RandUtils.FloatRange(0.5f, 1.2f, rand) +
                           RandUtils.FloatRange(0.5f, 1.2f, rand)) * 0.6f;

        if (zoneType.CreviceDepthScale > 0)
        {
            overallDepthMult *= zoneType.CreviceDepthScale;
        }

        LineGenParameters ld = GetCreviceParameters(sp, ep, zoneType, rand.Next(), 0);

        List<LineCell> line = _lineGenService.GetBressenhamLine(sp, ep, ld);

        if (line == null)
        {
            return;
        }





        List<LineCell> centerPoints = new List<LineCell>();

        foreach (LineCell item in line)
        {
            int cx = item.X;
            int cz = item.Z;
            if (cx < 0 || cx >= _mapProvider.GetMap().GetHwid() || cz < 0 || cz >= _mapProvider.GetMap().GetHhgt())
            {
                continue;
            }

            if (item.IsCenter)
            {
                centerPoints.Add(item);
            }
            float currDepthMult = 1.0f;

            if (depthOffsets != null)
            {
                int dx = cx - cdata.xStart;
                int dz = cz - cdata.zStart;

                if (dx >= 0 && dz >= 0 && dx < depthOffsets.GetLength(0) && dz < depthOffsets.GetLength(1))
                {
                    currDepthMult += depthOffsets[dx, dz];
                }

            }

            // Min depth to set crevice.
            _md.CreviceDepths[cx, cz] = Math.Min(_md.CreviceDepths[cx, cz], -1 * currDepthMult * overallDepthMult);



        }



        List<LineCell> sideCenterPoints = new List<LineCell>();

        int nextCreviceStart = RandUtils.IntRange(50, 90, rand);
        int nextCreviceMod = RandUtils.IntRange(40, 65, rand);

        float sideDepthMult = overallDepthMult * RandUtils.FloatRange(0.5f, 1.0f, rand);

        int nextCreviceDist = nextCreviceStart + crossRand.Next() % nextCreviceMod;
        int pointsSinceLastCrevice = nextCreviceStart + crossRand.Next() % nextCreviceMod;
        foreach (LineCell lc in centerPoints)
        {
            Point2I cp = new Point2I((int)(lc.X), (int)(lc.Z));
            pointsSinceLastCrevice++;
            if (pointsSinceLastCrevice < nextCreviceDist)
            {
                continue;
            }
            pointsSinceLastCrevice = 0;
            nextCreviceDist = nextCreviceStart + crossRand.Next() % nextCreviceMod;

            int cdx = sp.Z - ep.Z;
            int cdz = -(sp.X - ep.X);


            float origSize = MathUtil.Sqrt(cdx * cdx + cdz * cdz);
            if (origSize < 1)
            {
                continue;
            }
            int len = 40 + crossRand.Next() % 45;

            int newdx = (int)(len * cdx / origSize);
            int newdz = (int)(len * cdz / origSize);

            int maxdx = (int)(len * 1.0f);

            int csx = cp.X + newdx + RandUtils.IntRange(-maxdx, maxdx, crossRand);
            int csz = cp.Z - newdz + RandUtils.IntRange(-maxdx, maxdx, crossRand);
            int cex = cp.X + newdx + RandUtils.IntRange(-maxdx, maxdx, crossRand);
            int cez = cp.Z + newdz + RandUtils.IntRange(-maxdx, maxdx, crossRand);

            Point2I csp = new Point2I(csx, csz);
            Point2I cep = new Point2I(cex, cez);


            ld = GetCreviceParameters(csp, cep, zoneType, rand.Next(), 1);

            List<LineCell> line2 = _lineGenService.GetBressenhamLine(csp, cep, ld);
            foreach (LineCell pt2 in line2)
            {

                int cx = pt2.X;
                int cz = pt2.Z;
                if (cx < 0 || cx >= _mapProvider.GetMap().GetHwid() || cz < 0 || cz >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }

                float currDepthMult = 1.0f;

                if (depthOffsets != null)
                {
                    int dx = cx - cdata.xStart;
                    int dz = cz - cdata.zStart;

                    if (dx >= 0 && dz >= 0 && dx < depthOffsets.GetLength(0) && dz < depthOffsets.GetLength(1))
                    {
                        currDepthMult += depthOffsets[dx, dz];
                    }

                }

                // Min depth to create crevice.
                _md.CreviceDepths[cx, cz] = Math.Min(_md.CreviceDepths[cx, cz], -1 * currDepthMult * sideDepthMult);
                if (pt2.IsCenter)
                {
                    sideCenterPoints.Add(pt2);
                }

                line.Add(pt2);

            }
        }

        foreach (LineCell cp in sideCenterPoints)
        {
            centerPoints.Add(cp);
        }

        // Now force bridges to be made.

        for (int c = 0; c < centerPoints.Count; c++)
        {
            LineCell cp = centerPoints[c];
            int ax = (int)(cp.X * _mapProvider.GetMap().GetHwid() / _md.Awid);
            int az = (int)(cp.Z * _mapProvider.GetMap().GetHhgt() / _md.Ahgt);
            if (ax >= 0 && ax < _md.Awid && az >= 0 && az < _md.Ahgt)
            {
                if (_md.RoadDistances[ax, az] <= 2 && rand.NextDouble() < 0.03f)
                {
                    _md.CreviceBridges.Add(cp);
                }
            }
        }

        // now save smoothing for later.
        SmoothNearCrevice(zoneType, cdata, line, smoothOffsets);
    }


    private LineGenParameters GetCreviceParameters(Point2I sp, Point2I ep, ZoneType zoneType, int randomSeed, int depth)
    {

        LineGenParameters ld = new LineGenParameters();
        MyRandom rand = new MyRandom(randomSeed);


        int startWidth = 4 + rand.Next() % 3 + rand.Next() % 5;

        if (zoneType.CreviceWidthScale > 0)
        {
            startWidth = (int)(startWidth * zoneType.CreviceWidthScale);
            if (startWidth < 3)
            {
                startWidth = 3;
            }
        }

        if (depth > 0)
        {
            startWidth -= RandUtils.IntRange(startWidth / 4, startWidth * 2 / 3, rand);
        }

        ld.MinWidthSize = startWidth / 3;
        ld.WidthSize = startWidth;
        ld.MaxWidthSize = startWidth * 3;


        ld.WidthSizeChangeAmount = RandUtils.IntRange(2, 12, rand);

        ld.WidthSizeChangeChance = RandUtils.FloatRange(0.1, 0.3, rand);

        ld.WidthPosShiftChance = RandUtils.FloatRange(0.1f, 0.3f, rand);


        ld.WidthPosShiftSize = RandUtils.IntRange(2, 4, rand);

        ld.InitialNoPosShiftLength = RandUtils.IntRange(4, 8, rand);

        ld.MaxWidthPosDrift = RandUtils.FloatRange(0.2f, 0.8f, rand);

        ld.LinePathNoiseScale = RandUtils.FloatRange(0.0f, 1.1f, rand);
        ld.Seed = rand.Next();
        return ld;

    }

    public void SmoothNearCrevice(ZoneType zoneType, CreviceData cdata, List<LineCell> pts, float[,] smoothChanges)
    {
        if (_md.CreviceDepths == null || pts == null || cdata == null || zoneType == null)
        {
            return;
        }

        MyRandom smoothRand = new MyRandom(cdata.zone.Seed % 312323221 + 32423);

        float startSmoothRadius = SmoothRadiusDefault;
        float startRadiusDelta = SmoothRadiusDelta;

        startSmoothRadius *= RandUtils.FloatRange(0.8f, 1.3f, smoothRand);
        startRadiusDelta *= RandUtils.FloatRange(0.8f, 1.3f, smoothRand);

        // Loop through all points and make the depths approach 0 based on how far they are from the points in the line.
        // Use min of currval, smooth val to keep the crevices in place.
        foreach (LineCell pt in pts)
        {
            int cx = (int)(pt.X);
            int cz = (int)(pt.Z);

            if (cx < 0 || cz < 0 || cx >= _mapProvider.GetMap().GetHwid() || cz >= _mapProvider.GetMap().GetHhgt())
            {
                continue;
            }

            float centerDepth = _md.CreviceDepths[cx, cz];
            float smoothRadius = startSmoothRadius;

            if (zoneType.CreviceWidthScale > 0)
            {
                smoothRadius *= zoneType.CreviceWidthScale;
            }

            if (smoothChanges != null)
            {
                int dx = cx - cdata.xStart;
                int dz = cz - cdata.zStart;

                if (dx >= 0 && dz >= 0 && dx < smoothChanges.GetLength(0) && dz < smoothChanges.GetLength(1))
                {
                    smoothRadius += smoothChanges[dx, dz] * startSmoothRadius;
                }
                smoothRadius = MathUtil.Clamp(startSmoothRadius / 2, smoothRadius, startSmoothRadius * 2);
            }

            int smoothRadiusInt = (int)(smoothRadius);

            for (int xx = cx - smoothRadiusInt; xx <= cx + smoothRadiusInt; xx++)
            {
                if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                {
                    continue;
                }
                float ddx = cx - xx;
                for (int zz = cz - smoothRadiusInt; zz <= cz + smoothRadiusInt; zz++)
                {
                    if (zz < 0 || zz >= _mapProvider.GetMap().GetHhgt())
                    {
                        continue;
                    }
                    float ddz = cz - zz;

                    float dist = (float)Math.Sqrt(ddx * ddx + ddz * ddz);
                    if (dist >= smoothRadius)
                    {
                        continue;
                    }

                    float distMult = 1 - dist / smoothRadius;

                    float valueToSet = centerDepth * distMult;

                    float currVal = _md.CreviceDepths[xx, zz];

                    if (valueToSet < currVal)
                    {
                        _md.CreviceDepths[xx, zz] = valueToSet;
                    }
                }
            }
        }
    }
}


