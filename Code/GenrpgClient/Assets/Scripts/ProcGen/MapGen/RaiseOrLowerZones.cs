using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ZoneHeightCellData
{

    public int zoneId;
    public int x;
    public int z;
    public int wx;
    public int wz;
    public int heightOffset;
}

public class RaiseOrLowerZones : BaseZoneGenerator
{

    public const int StartDist = -1000;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed % 23432432 + 31234);

        int minEdgeDistance = _mapProvider.GetMap().BlockCount / 5 * MapConstants.TerrainPatchSize;

        if (minEdgeDistance > 5 * MapConstants.TerrainPatchSize)
        {
            minEdgeDistance = 5 * MapConstants.TerrainPatchSize;
        }

        if (minEdgeDistance < 2 * MapConstants.TerrainPatchSize)
        {
            // return;
        }

        int minpos = minEdgeDistance;
        int maxPos = _mapProvider.GetMap().GetHwid() - minEdgeDistance;
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {

            if (zone.MinX >= zone.MaxX || zone.MinZ > zone.MaxZ)
            {
                continue;
            }

            if (zone.MinX < minpos || zone.MinZ < minpos || zone.MaxX > maxPos || zone.MaxZ > maxPos)
            {
                continue;
            }


            RaiseOrLowerZone(zone, rand.Next());
        }

    }

    private void RaiseOrLowerZone(Zone zone, int seed)
    {
        if (_mapProvider.GetMap() == null || zone == null)
        {
            return;
        }

        int roadCheckRad = 5;

        int extraWidth = (int)_md.GetMountainDefaultSize(_mapProvider.GetMap());

        MyRandom rand = new MyRandom(seed);


        extraWidth = RandUtils.IntRange(extraWidth * 4 / 5, extraWidth * 5 / 4, rand);


        float heightOffset = RandUtils.FloatRange(0.7f, 0.9f, rand) * extraWidth / MapConstants.MapHeight;


        float waterScaledHeight = 1.0f * MapConstants.MinLandHeight / MapConstants.MapHeight;

        int midx = (zone.MinX + zone.MaxX) / 2;
        int midz = (zone.MinZ + zone.MaxZ) / 2;

        float midHeight = _md.Heights[midx, midz];

        float centerSpread = 0.3f;

        float powerSpread = 0.3f;

        float minCenter = 0.5f - centerSpread;
        float maxCenter = 0.5f + centerSpread;
        float minPower = 1 - powerSpread;
        float maxPower = 1 + powerSpread;

        int minx = (int)Math.Max(0, zone.MinX - extraWidth);
        int maxx = (int)Math.Min(_mapProvider.GetMap().GetHwid() - 1, zone.MaxX + extraWidth);
        int minz = (int)Math.Max(0, zone.MinZ - extraWidth);
        int maxz = (int)Math.Min(_mapProvider.GetMap().GetHhgt() - 1, zone.MaxZ + extraWidth);

        int closeCheckEdgeSize = 8;

        bool tooLowAlready = false;

        bool tooCloseToRaisedOrLowered = false;
        for (int x = minx + closeCheckEdgeSize; x < maxx - closeCheckEdgeSize; x++)
        {
            for (int z = minz + closeCheckEdgeSize; z < maxz - closeCheckEdgeSize; z++)
            {
                if (FlagUtils.MatchesAnyBits(_md.Flags[x, z], MapGenFlags.IsRaisedOrLowered))
                {
                    tooCloseToRaisedOrLowered = true;
                    break;
                }

                if (x >= minx && x < maxx && z >= minz && z < maxz)
                {
                    if (_md.Heights[x, z] - heightOffset < waterScaledHeight)
                    {
                        tooLowAlready = true;
                    }
                }
            }
            if (tooCloseToRaisedOrLowered)
            {
                break;
            }
        }

        if (tooCloseToRaisedOrLowered)
        {
            return;
        }

        int distx = maxx - minx + 1;
        int distz = maxz - minz + 1;

        int size = (distx + distz) / 2;

        if (distx < 100 || distz < 100)
        {
            return;
        }

        int noLowEdgeSize = MapConstants.TerrainPatchSize * 10;


        if (midx < noLowEdgeSize ||
            midz < noLowEdgeSize ||
            midx > _mapProvider.GetMap().GetHwid() - noLowEdgeSize ||
            midz > _mapProvider.GetMap().GetHhgt() - noLowEdgeSize)
        {
            tooLowAlready = true;
        }


        if (rand.NextDouble() < 0.5 && !tooLowAlready)
        {
            heightOffset = -heightOffset;
        }

        float centerAmp = RandUtils.FloatRange(powerSpread, powerSpread * 2, rand);
        float centerFreq = RandUtils.FloatRange(size / 40, size / 10, rand);
        float centerPers = RandUtils.FloatRange(0.1f, 0.4f, rand);
        int centerOctaves = 2;

        float[,] centers = _noiseService.Generate(centerPers, centerFreq, centerAmp, centerOctaves, rand.Next(), distx, distz);

        float powerAmp = RandUtils.FloatRange(centerSpread, centerSpread * 2, rand);
        float powerFreq = RandUtils.FloatRange(size / 40, size / 10, rand);
        float powerPers = RandUtils.FloatRange(0.1f, 0.3f, rand);
        int powerOctaves = 2;

        float[,] powers = _noiseService.Generate(powerPers, powerFreq, powerAmp, powerOctaves, rand.Next(), distx, distz);

        int xsize = maxx - minx + 1;
        int zsize = maxz - minz + 1;

        int[,] distances = new int[xsize, zsize];

        for (int x = 0; x < xsize; x++)
        {
            for (int z = 0; z < zsize; z++)
            {
                distances[x, z] = StartDist;
            }
        }

        // Now get the 0 dist zones.

        Queue<ZoneHeightCellData> cellQueue = new Queue<ZoneHeightCellData>();

        List<Point2I> offsetList = new List<Point2I>();
        offsetList.Add(new Point2I(-1, 0));
        offsetList.Add(new Point2I(1, 0));
        offsetList.Add(new Point2I(0, 1));
        offsetList.Add(new Point2I(0, -1));
        for (int x = 0; x < xsize; x++)
        {
            int wx = x + minx;
            if (wx < 1 || wx >= _mapProvider.GetMap().GetHwid() - 1)
            {
                continue;
            }

            for (int z = 0; z < zsize; z++)
            {
                int wz = z + minz;
                if (wz < 1 || wz >= _mapProvider.GetMap().GetHhgt() - 1)
                {
                    continue;
                }

                if (_md.MapZoneIds[wx, wz] != zone.IdKey)
                {
                    continue;
                }

                foreach (Point2I offset in offsetList)
                {
                    int x2 = wx + (int)(offset.X);
                    int z2 = wz + (int)(offset.Z);

                    if (_md.MapZoneIds[x2, z2] != zone.IdKey)
                    {

                        ZoneHeightCellData cell = new ZoneHeightCellData()
                        {
                            zoneId = _md.MapZoneIds[wx, wz],
                            wx = wx,
                            wz = wz,
                            x = x,
                            z = z,
                            heightOffset = 0,
                        };
                        cellQueue.Enqueue(cell);
                        distances[x, z] = 0;
                        break;
                    }
                }
            }
        }


        while (cellQueue.Count > 0)
        {
            ZoneHeightCellData firstCell = cellQueue.Dequeue();

            if (firstCell.wx < 1 || firstCell.wx >= _mapProvider.GetMap().GetHwid() - 1 ||
                firstCell.wz < 1 || firstCell.wz >= _mapProvider.GetMap().GetHhgt() - 1 ||
                firstCell.x < 1 || firstCell.x >= xsize - 1 ||
                firstCell.z < 1 || firstCell.z >= zsize - 1)
            {
                continue;
            }

            foreach (Point2I offset in offsetList)
            {
                int nwx = firstCell.wx + (int)(offset.X);
                int nwz = firstCell.wz + (int)(offset.Z);
                int nx = firstCell.x + (int)(offset.X);
                int nz = firstCell.z + (int)(offset.Z);
                if (distances[nx, nz] != StartDist)
                {
                    continue;
                }

                short newZoneId = _md.MapZoneIds[nwx, nwz];

                int delta = 0;

                if (newZoneId == zone.IdKey) // Height goes up.
                {
                    delta = 1;
                }
                else
                {
                    delta = -1;
                }

                int newHeightOffset = firstCell.heightOffset + delta;

                ZoneHeightCellData cell = new ZoneHeightCellData()
                {
                    zoneId = newZoneId,
                    wx = nwx,
                    wz = nwz,
                    x = nx,
                    z = nz,
                    heightOffset = newHeightOffset,
                };
                cellQueue.Enqueue(cell);
                distances[nx, nz] = newHeightOffset;
            }

        }


        // Now we have all of these numbers from -extrawidth to extraWidth now raiselower the hills.


        float middleHeightPct = 0.40f;

        float deltaDiv = 8.0f;
        int numCellsChanged = 0;
        for (int x = 0; x < xsize; x++)
        {
            for (int z = 0; z < zsize; z++)
            {
                int nx = x;
                int nz = z;

                int xroadDelta = 0;
                int zroadDelta = 0;

                // Check for roads nearby...if more are "farther" away from center, lower this
                // otherwise raise this.
                for (int xx = x - roadCheckRad; xx <= x + roadCheckRad; xx++)
                {
                    if (xx < 0 || xx >= xsize)
                    {
                        continue;
                    }

                    int drx = xx - x;
                    int wx = xx + minx;
                    if (wx < 0 || wx >= _mapProvider.GetMap().GetHwid())
                    {
                        continue;
                    }

                    for (int zz = z - roadCheckRad; zz <= z + roadCheckRad; zz++)
                    {
                        if (zz < 0 || zz >= zsize)
                        {
                            continue;
                        }

                        int drz = zz - z;
                        int wz = zz + minz;
                        if (wz < 0 || wz >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }

                        // Use wx wz for global alphas value
                        if (_md.Alphas[wx, wz, TerrainTexChannels.Road] > 0)
                        {
                            if (xx < x)
                            {
                                xroadDelta--;
                            }
                            else if (xx > x)
                            {
                                xroadDelta++;
                            }

                            if (zz < z)
                            {
                                zroadDelta--;
                            }
                            else if (zz > z)
                            {
                                zroadDelta++;
                            }
                        }
                    }
                }
                nx = MathUtil.Clamp(0, x + (int)(xroadDelta / deltaDiv), xsize - 1);
                nz = MathUtil.Clamp(0, z + (int)(zroadDelta / deltaDiv), zsize - 1);

                int currDist = distances[nx, nz];

                if (currDist > -extraWidth && currDist < extraWidth)
                {
                    if (currDist != 0)
                    {
                        numCellsChanged++;
                    }
                }

                currDist = MathUtil.Clamp(-extraWidth, currDist, extraWidth);

                float heightDistPct = 0.0f;

                if (currDist <= 0)
                {
                    // 0 at -extra width, middleHeightPercent at middle.
                    heightDistPct = ((currDist + extraWidth) * middleHeightPct) / extraWidth;
                }
                else
                {
                    // start at middleHeightPercent at 0, up to 1 at extraWidth
                    heightDistPct = middleHeightPct + currDist * (1 - middleHeightPct) / extraWidth;
                }
                float power = MathUtil.Clamp(minPower, 1.0f + powers[x, z], maxPower);

                float powerDistPct = (float)(Math.Pow(heightDistPct, power));

                float finalHeightOffset = powerDistPct * heightOffset;

                _md.Flags[x + minx, z + minz] |= MapGenFlags.IsRaisedOrLowered;
                _md.Heights[x + minx, z + minz] += finalHeightOffset;
            }
        }

    }
}

