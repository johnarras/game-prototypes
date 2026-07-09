
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Settings.Fences;
using OxDb.SharedGame.ProcGen.Settings.Locations;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AddFences : BaseZoneGenerator
{
    public const int NumCentersAveraged = 8;

    const float MaxFenceHeightAngle = 20;

    public override async Awaitable Generate(CancellationToken token)
    {

        await base.Generate(token);
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.MinX, zone.MinZ, zone.MaxX, zone.MaxZ);
        }

    }

    public void GenerateOne(Zone zone, ZoneType zoneType, int startx, int startz, int endx, int endz)
    {
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
            endx = _mapProvider.GetMap().GetHwid();
        }

        if (endz >= _mapProvider.GetMap().GetHhgt())
        {
            endz = _mapProvider.GetMap().GetHhgt();
        }

        List<Point3F> fences = new List<Point3F>();
        if (zoneType.FenceTypes == null || zoneType.FenceTypes.Count < 1)
        {
            return;
        }

        if (_gameData.Get<FenceTypeSettings>(_gs.ch).GetData() == null || _gameData.Get<FenceTypeSettings>(_gs.ch).GetData().Count < 1)
        {
            return;
        }

        MyRandom chanceRand = new MyRandom(zone.Seed % 100000000 + 234323);

        MyRandom choiceRand = new MyRandom(zone.Seed % 100000000 + 9972367);

        MyRandom placeRand = new MyRandom(zone.Seed % 82348732 + 33234);

        int distFromEnd = 6;

        List<Point3F> currFences = new List<Point3F>();

        float minDistToFence = 2.5f;

        int xsize = endx - startx + 1;
        int zsize = endz - startz + 1;

        float amp = RandUtils.FloatRange(0.0f, 0.3f, chanceRand);
        float freq = RandUtils.FloatRange(0.02f, 0.1f, chanceRand) * (xsize + zsize) / 2;
        float pers = RandUtils.FloatRange(0.2f, 0.45f, chanceRand);
        int octaves = 2;

        float[,] fenceChances = _noiseService.Generate(pers, freq, amp, octaves, chanceRand.Next(), xsize, zsize);



        float maxHeightAboveCenter = 1.0f;

        for (int x = startx + distFromEnd; x < endx - distFromEnd; x++)
        {
            int ddx = x - startx;
            for (int z = startz + distFromEnd; z < endz - distFromEnd; z++)
            {
                int ddz = z - startz;
                if (chanceRand.NextDouble() > fenceChances[ddx, ddz])
                {
                    Location currLoc = _zoneGenService.FindMapLocation(x, z, 3);

                    if (currLoc == null ||
                        FlagUtils.MatchesAnyBits(_md.Flags[x, z], MapGenFlags.IsLocationPatch))
                    {
                        continue;
                    }
                }

                if (_md.MapZoneIds[x, z] != zone.IdKey) // zoneobject
                {
                    continue;
                }
                float startRoadDist = _md.RoadDistances[x, z];
                if (startRoadDist < 1.5f || startRoadDist > 2.5f)
                {
                    continue;
                }

                if (FlagUtils.MatchesAnyBits(_md.Flags[x, z], MapGenFlags.BelowWater))
                {
                    continue;
                }

                FenceType fenceType = GetFenceType(zoneType, choiceRand);
                if (fenceType == null)
                {
                    continue;
                }

                bool closeToFence = false;
                foreach (Point3F item in currFences)
                {
                    float dx2 = item.X - x;
                    float dz2 = item.Z - z;
                    if (Math.Sqrt(dx2 * dx2 + dz2 * dz2) < minDistToFence)
                    {
                        closeToFence = true;
                        continue;
                    }
                }

                if (closeToFence)
                {
                    continue;
                }

                List<Point3F> potentialEndPoints = new List<Point3F>();

                float extraLengthMult = 2.0f;

                float checkLength = fenceType.Length * extraLengthMult;

                int radToCheck = (int)Math.Ceiling(checkLength);

                for (int xx = x - radToCheck; xx <= x + radToCheck; xx++)
                {
                    if (xx < startx || xx >= endx)
                    {
                        continue;
                    }
                    for (int zz = z - radToCheck; zz <= z + radToCheck; zz++)
                    {
                        if (zz < startz || zz >= endz)
                        {
                            continue;
                        }

                        float dist = (float)Math.Sqrt((double)((xx - x) * (xx - x) + (zz - z) * (zz - z)));
                        if (dist < radToCheck - 0.5f || dist > radToCheck + 0.5f)
                        {
                            continue;
                        }
                        float newDist = _md.RoadDistances[xx, zz];
                        if (newDist < startRoadDist - 0.5f || newDist > startRoadDist + 0.5f)
                        {
                            continue;
                        }

                        int sx = Math.Min((int)x, xx) + 1;
                        int ex = Math.Max((int)x, xx) - 1;
                        int sz = Math.Min(z, zz) + 1;
                        int ez = Math.Max(z, zz) - 1;

                        bool tooCloseToRoad = false;

                        for (int vx = sx; vx <= ex; vx++)
                        {
                            for (int vz = sz; vz <= sz; vz++)
                            {
                                if (_md.RoadDistances[vx, vz] < 0.5f)
                                {
                                    tooCloseToRoad = true;
                                    break;
                                }
                            }
                        }

                        if (tooCloseToRoad)
                        {
                            continue;
                        }

                        float enx = (x + (xx - x) / extraLengthMult);
                        float enz = (z + (zz - z) / extraLengthMult);

                        int inx = (int)(enx);
                        int inz = (int)(enz);

                        float ird = _md.RoadDistances[inx, inz];

                        if (ird < startRoadDist - 0.75f || ird > startRoadDist + 0.75f)
                        {
                            continue;
                        }

                        if (_md.BridgeDistances[xx, zz] < 15)
                        {
                            continue;
                        }

                        potentialEndPoints.Add(new Point3F((float)enx, enz));
                    }
                }

                if (potentialEndPoints.Count < 1)
                {
                    continue;
                }

                Point3F chosenEndPt = potentialEndPoints[choiceRand.Next() % potentialEndPoints.Count];

                float slope = _terrainManager.GetSteepness(x, z);

                if (slope > 30)
                {
                    continue;
                }

                int wdx = x / (MapConstants.TerrainPatchSize - 1);
                int wdz = z / (MapConstants.TerrainPatchSize - 1);

                float cx = (x + chosenEndPt.X) / 2.0f;
                float cz = (z + chosenEndPt.Z) / 2.0f;

                float dz = chosenEndPt.Z - z;
                float dx = chosenEndPt.X - x;
                float angle = (float)(Math.Atan2(dz, dx) * 180f / Math.PI + 90);

                //float hgt = _md.SampleHeight(x+wdx, 2000, z+wdz);
                float hgt = _terrainManager.SampleHeight(z + wdz, x + wdx);

                //var centerHeight = _md.SampleHeight(cx+wdx, 2000, cz+wdz);
                float centerHeight = _terrainManager.SampleHeight(cz + wdz, cx + wdx);

                if (Math.Abs(hgt - centerHeight) > maxHeightAboveCenter)
                {
                    continue;
                }

                float dhx = (float)Math.Sqrt((cx - x) * (cx - x) + (cz - z) * (cz - z));
                float dhz = centerHeight - hgt;

                float hangle = (float)(Math.Atan2(-dhz, dhx) * 180 / Math.PI);

                // Don't allow fences that are too slanted. Looks bad.
                if (Math.Abs(hangle) >= MaxFenceHeightAngle)
                {
                    continue;
                }

                int intAngle = MathUtil.ModClamp((int)angle, MapConstants.FullCircleAngle);
                int intHangle = MathUtil.ModClamp((int)hangle, MapConstants.FullCircleAngle);

                if (_md.SetEntityData(x, z, EntityTypes.Fence, fenceType.IdKey))
                {
                    _md.ExtendedObjects[x, z] = new ExtendedWorldObjectData()
                    {
                        X = x,
                        Z = z,
                        EntityTypeId = EntityTypes.Fence,
                        EntityId = fenceType.IdKey,
                        Angle = (ushort)intAngle,
                        HAngle = (ushort)intHangle,

                    };

                    currFences.Add(new Point3F((float)x, z, 0));
                }

            }
        }
    }

    public float GetHeightAngle(FenceType fenceType, float angle, float newx, float newz, float hgt)
    {
        if (_gs == null || fenceType == null)
        {
            return 0.0f;
        }

        float fenceLength = fenceType.Length;

        float endx = (float)(newx + Math.Cos((angle - 90) * Math.PI / 180) * fenceLength);
        float endz = (float)(newz + Math.Sin((angle - 90) * Math.PI / 180) * fenceLength);


        int eax = (int)((endx / _mapProvider.GetMap().GetHwid()) * _md.Awid);
        int eay = (int)((endz / _mapProvider.GetMap().GetHhgt()) * _md.Ahgt);


        if (_md.RoadDistances[(int)endx, (int)endz] <= 1)
        {
            return 0.0f;
        }


        float endhgt = _terrainManager.SampleHeight(endx, endz);





        float hdy = -(endhgt - hgt);
        float hdx = fenceLength;

        float hangle = (float)(Math.Atan2(hdy, hdx) * 180 / Math.PI);

        return hangle;
    }

    private FenceType GetFenceType(ZoneType ztype, MyRandom choiceRand)
    {
        if (choiceRand == null || ztype == null)
        {
            return null;
        }

        ZoneFenceType zoneFenceType = RandUtils.GetRandomElement(ztype.FenceTypes, choiceRand);
        if (zoneFenceType == null)
        {
            return null;
        }

        FenceType fenceType = _gameData.Get<FenceTypeSettings>(_gs.ch).Get(zoneFenceType.FenceTypeId);

        if (fenceType == null || string.IsNullOrEmpty(fenceType.Art))
        {
            return null;
        }

        return fenceType;
    }

}



