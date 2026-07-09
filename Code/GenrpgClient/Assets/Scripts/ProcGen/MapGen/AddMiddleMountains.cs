
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AddMiddleMountains : BaseAddMountains
{
    public override async Awaitable Generate(CancellationToken token)
    {
        AddMiddleMapMountains(_gs);
        await Task.CompletedTask;
    }



    public void AddMiddleMapMountains(IClientGameState gs)
    {
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            AddMiddleZoneMountains(zone);

        }

        // AddDungeonMountains(gs);
    }

    protected void AddMiddleZoneMountains(Zone zone)
    {
        if (_gs == null || zone == null || _md == null || _md.MaintainHeights == null)
        {
            return;
        }

        int xsize = zone.MaxX - zone.MinX;
        int zsize = zone.MaxZ - zone.MinZ;
        int maxSize = Math.Max(xsize, zsize);
        int minSize = MapConstants.TerrainPatchSize * 5;

        if (xsize < minSize || zsize < minSize)
        {
            return;
        }

        MyRandom middleRand = new MyRandom(zone.Seed + 233499);


        int numWalls = RandUtils.IntRange(0, 1, middleRand);

        if (middleRand.NextDouble() < 0.2f)
        {
            numWalls++;
        }

        int wallsAdded = 0;
        for (int times = 0; times < 200; times++)
        {

            if (wallsAdded >= numWalls)
            {
                break;
            }

            int currMaxLen = RandUtils.IntRange(30, maxSize, middleRand);


            int sx = RandUtils.IntRange(zone.MinX, zone.MaxX, middleRand);

            int sz = RandUtils.IntRange(zone.MinZ, zone.MaxZ, middleRand);


            int ex = RandUtils.IntRange(sx - currMaxLen, sx + currMaxLen, middleRand);

            int ez = RandUtils.IntRange(sz - currMaxLen, sx + currMaxLen, middleRand);

            if (ex < 0 || ex >= _mapProvider.GetMap().GetHwid() || ez < 0 || ez >= _mapProvider.GetMap().GetHhgt())
            {
                continue;
            }

            if (_md.MapZoneIds[sx, sz] != zone.IdKey || _md.MapZoneIds[ex, ez] != zone.IdKey)
            {
                continue;
            }

            int dx = Math.Abs(ex - sx);
            int dz = Math.Abs(ez - sz);

            int maxDist = Math.Max(dx, dz);

            if (maxDist >= 10 && maxDist < maxSize)
            {
                float height = GetMountainHeightMult(middleRand);
                AddMountainRidge(sx, sz, ex, ez, zone.Seed / 2 + times, false, height, true);
                wallsAdded++;
            }
        }
    }
}

