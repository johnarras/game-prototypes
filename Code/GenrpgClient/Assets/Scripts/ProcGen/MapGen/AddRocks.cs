using OxDb.Client.Assets.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Entities;
using OxDb.SharedGame.ProcGen.Settings.Rocks;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

internal class FullRockType
{
    public RockType rockType;
    public WeightedEntity zoneTypeRock;
    public int numPlaced;

    public double Weight;


    public List<Point2F> PlacedRocks;

    public string assetCategory = AssetCategoryNames.Rocks;

    public string assetName = "";
    public string fullURL = "";
    public FullRockType()
    {
        PlacedRocks = new List<Point2F>();
    }
}

public class AddRocks : BaseZoneGenerator
{
    public const float RandomRockDensity = 1.0f / 4000.0f;
    public int TriesPerRock = 20;

    IAddNearbyItemsHelper _addNearbyItemsHelper;
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
        GenZone genZone = _md.GetGenZone(zone.IdKey);

        if (endx <= startx || endz <= startz)
        {
            return;
        }

        int dx = endx - startx;
        int dz = endz - startz;

        MyRandom rand = new MyRandom(zone.Seed % 2000000000 + 15434454);


        float densityMult = RandUtils.FloatRange(1.0f, 1.5f, rand);

        List<FullRockType> list = new List<FullRockType>();


        List<WeightedEntity> rockTypes = genZone.GetPropsOfType(EntityTypes.Rock);

        foreach (WeightedEntity zrt in rockTypes)
        {

            RockType rt = _gameData.Get<RockTypeSettings>(_gs.ch).Get(zrt.EntityId);
            if (rt == null)
            {
                continue;
            }

            if (rt.ChanceScale <= 0.0f || zrt.Weight <= 0)
            {
                continue;
            }

            float weight = (float)(rt.ChanceScale * zrt.Weight);

            if (weight <= 0)
            {
                continue;
            }

            FullRockType full = new FullRockType();
            full.zoneTypeRock = zrt;
            full.rockType = rt;
            full.Weight = weight;
            full.assetName = rt.Name;
            full.fullURL = full.assetName;
            full.assetCategory = AssetCategoryNames.Rocks;
            list.Add(full);
        }

        int size = Math.Max(zone.MaxX - zone.MinX, zone.MaxZ - zone.MinZ);

        long area = (zone.MaxX - zone.MinX) * (zone.MaxZ - zone.MinZ);

        long totalNumber = (long)((area * RandomRockDensity) * zoneType.RockDensity * densityMult);

        long totalTries = (long)(totalNumber * TriesPerRock);

        int totalPlaced = 0;
        for (long times = 0; times < totalTries; times++)
        {

            if (totalPlaced >= totalNumber)
            {
                break;
            }

            int x = RandUtils.IntRange(startx, endx, rand);
            int z = RandUtils.IntRange(startz, endz, rand);



            if (_zoneGenService.FindMapLocation(x, z, 10) != null)
            {
                continue;
            }


            if (_md.MapZoneIds[x, z] != zone.IdKey) // zoneobject
            {
                continue;
            }

            if (_md.RoadDistances[x, z] < 10)
            {
                continue;
            }


            int currQuantityToPlace = 1;

            while (rand.NextDouble() < 0.2f && rand.Next() % currQuantityToPlace < 2)
            {
                currQuantityToPlace += rand.Next() % 3 + 1;
            }

            int maxOffset = currQuantityToPlace / 3;

            bool didFinalPlace = false;

            List<long> placedList = new List<long>();
            for (int p = 0; p < currQuantityToPlace; p++)
            {

                int nearbyItemsCount = _addNearbyItemsHelper.GetNearbyItemsCount(maxOffset, rand);

                FullRockType frt = null;

                double totalChance = 0;
                foreach (FullRockType item in list)
                {
                    if (placedList.Contains(item.rockType.IdKey) &&
                        item.rockType.MaxPerZone > 0)
                    {
                        continue;
                    }

                    totalChance += item.Weight;

                }

                if (totalChance <= 0)
                {
                    if (list.Count < 1)
                    {
                        break;
                    }
                    frt = list[rand.Next() % list.Count];

                }
                else
                {
                    double chanceChosen = rand.NextDouble() * totalChance;

                    foreach (FullRockType item in list)
                    {
                        if (item.rockType.MaxPerZone > 0 && placedList.Contains(item.rockType.IdKey))
                        {
                            continue;
                        }
                        chanceChosen -= item.Weight;
                        if (chanceChosen <= 0)
                        {
                            frt = item;
                            break;
                        }
                    }

                }

                if (frt == null)
                {
                    continue;
                }


                int px = x + RandUtils.IntRange(-maxOffset, maxOffset, rand);
                int pz = z + RandUtils.IntRange(-maxOffset, maxOffset, rand);

                px -= px / (MapConstants.TerrainPatchSize - 1);
                pz -= pz / (MapConstants.TerrainPatchSize - 1);

                int rdx = px - x;
                int rdz = pz - z;

                float rdist = (float)Math.Sqrt(rdx * rdx + rdz * rdz);

                int ipx = (int)(px);
                int ipz = (int)(pz);

                if (ipx < 0 || ipz < 0 || ipx >= _mapProvider.GetMap().GetHwid() || ipz >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }

                if (_md.RoadDistances[ipx, ipz] < 3)
                {
                    continue;
                }
                float posHeight = _terrainManager.GetInterpolatedHeight(ipx, ipz);

                if (posHeight < MapConstants.MinLandHeight)
                {
                    continue;
                }

                if (!_md.CellHasObject(ipx, ipz))
                {
                    _md.SetEntityData(ipx, ipz, EntityTypes.Rock, frt.rockType.IdKey);

                    didFinalPlace = true;

                    if (rand.NextDouble() * currQuantityToPlace > densityMult)
                    {
                        int numToPlace = 8 - (currQuantityToPlace + 1) / 2;
                        if (numToPlace < 3)
                        {
                            numToPlace = 3;
                        }

                        float currMaxOffset = RandUtils.FloatRange(1.1f, 2.1f, rand);
                        float currMinOffset = currMaxOffset / 2;
                        _addNearbyItemsHelper.AddItemsNear(rand, zoneType, zone, x, z, 0.9f, nearbyItemsCount, currMinOffset, currMaxOffset);
                    }
                }

                if (!placedList.Contains(frt.rockType.IdKey))
                {
                    placedList.Add(frt.rockType.IdKey);
                }

                frt.numPlaced++;
                if (frt.rockType.MaxPerZone > 0 && frt.numPlaced >= frt.rockType.MaxPerZone)
                {
                    list.Remove(frt);
                }
            }

            if (didFinalPlace)
            {
                totalPlaced++;
            }
        }
    }
}

