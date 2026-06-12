
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AddEdgeMountains : BaseAddMountains
{
    public override async Awaitable Generate(CancellationToken token)
    {

        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed / 4 + 31433);
        short[,] zoneIds = _md.MapZoneIds;
        List<MyPointF> points = new List<MyPointF>();
        int mountainWidth = (int)_md.GetMountainDefaultSize(_mapProvider.GetMap());
        int edgeWidth = mountainWidth + MapConstants.TerrainPatchSize / 4;

        int mapSize = _mapProvider.GetMap().GetHwid();
        float heightFreq = mapSize * 1.0f / (MapConstants.TerrainPatchSize / 2);
        float heightAmp = 0.6f;
        float heightPers = 0.4f;
        int heightOctaves = 2;

        float[,] heights = _noiseService.Generate(heightPers, heightFreq, heightAmp, heightOctaves, rand.Next(), mapSize, mapSize);


        float widthFreq = mapSize * 1.0f / (MapConstants.TerrainPatchSize / 2);
        float widthAmp = 0.7f;
        float widthPers = 0.4f;
        int widthOctaves = 2;

        float[,] widths = _noiseService.Generate(widthPers, widthFreq, widthAmp, widthOctaves, rand.Next(), mapSize, mapSize);

        int startMountainWidth = (int)_md.GetMountainDefaultSize(_mapProvider.GetMap());

        for (int x = edgeWidth; x < _mapProvider.GetMap().GetHwid() - 1 - edgeWidth; x++)
        {
            for (int z = edgeWidth; z < _mapProvider.GetMap().GetHhgt() - 1 - edgeWidth; z++)
            {
                if (zoneIds[x, z] != zoneIds[x + 1, z] ||
                    zoneIds[x, z] != zoneIds[x, z + 1] ||
                    zoneIds[x, z] != zoneIds[x + 1, z + 1])
                {
                    float heightVal = Math.Max(heights[x, z] + 0.5f, 0);
                    if (heightVal > 0)
                    {
                        _md.Flags[x, z] |= MapGenFlags.IsWallCenter;
                        _md.MaintainHeights[x, z] = heightVal;
                        points.Add(new MyPointF(x, z));
                    }
                }
            }
        }

        for (int l = 0; l < points.Count; l++)
        {
            MyPointF item = points[l];
            int cx = (int)(item.X);
            int cz = (int)(item.Y);
            if (cx < 0 || cz < 0 || cx >= _mapProvider.GetMap().GetHwid() || cz >= _mapProvider.GetMap().GetHhgt())
            {
                continue;
            }
            float mainHeight = heights[cx, cz];

            int currWallWidth = (int)(startMountainWidth * (1 + widths[cx, cz]));

            if (_md.MapZoneIds[cx, cz] < 1)
            {
                _md.MapZoneIds[cx, cz] = MapConstants.MountainZoneId;
            }


            _md.MountainDistPercent[cx, cz] = 0f;
            _md.EdgeMountainDistPercent[cx, cz] = 0f;

            float topWidth = 2;
            int mincmidx = Math.Min(_mapProvider.GetMap().GetHwid() / 2, cx);
            int maxcmidx = Math.Max(_mapProvider.GetMap().GetHwid() / 2, cx);
            int mincmidy = Math.Min(_mapProvider.GetMap().GetHhgt() / 2, cz);
            int maxcmidy = Math.Max(_mapProvider.GetMap().GetHhgt() / 2, cz);
            for (int y = Math.Max(0, cz - currWallWidth); y <= Math.Min(_mapProvider.GetMap().GetHhgt() - 1, cz + currWallWidth); y++)
            {
                int ddy = Math.Abs(y - cz);
                for (int x = Math.Max(0, cx - currWallWidth); x <= Math.Min(_mapProvider.GetMap().GetHwid() - 1, cx + currWallWidth); x++)
                {
                    int ddx = Math.Abs(x - cx);

                    double currDist = Math.Sqrt(ddx * ddx + ddy * ddy);
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

                    if (_md.MapZoneIds[x, y] == 0)
                    {
                        _md.MapZoneIds[x, y] = MapConstants.MountainZoneId;
                    }
                    if (_md.MountainDistPercent[x, y] > distPct)
                    {
                        _md.MountainDistPercent[x, y] = (float)distPct;
                    }
                    if (_md.MountainCenterDist[x, y] > currDist)
                    {
                        _md.MountainCenterDist[x, y] = (float)(currDist);
                        _md.NearestMountainTopHeight[x, y] = mainHeight;
                    }
                    if (_md.EdgeMountainDistPercent[x, y] > distPct)
                    {
                        _md.EdgeMountainDistPercent[x, y] = (float)distPct;
                    }
                    _md.Flags[x, y] |= MapGenFlags.IsEdgeWall;

                    float currPower = MathUtil.Clamp(0.5f, 1.7f, 1.0f + _md.MountainDecayPower[x, y]);
                    float newPct = _md.MaintainHeights[cx, cz] * (float)(1.0f - Math.Pow(distPct, currPower));

                    if (newPct != 0 && _md.MaintainHeights[x, y] == 0)
                    {
                        _md.Flags[x, y] |= MapGenFlags.IsSecondaryWall;
                    }

                    if (newPct > _md.MaintainHeights[x, y])
                    {
                        _md.MaintainHeights[x, y] = newPct;
                    }
                }
            }
        }
        await Task.CompletedTask;
    }
}

