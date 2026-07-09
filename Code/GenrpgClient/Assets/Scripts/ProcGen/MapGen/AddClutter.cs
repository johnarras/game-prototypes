using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.ProcGen.Settings.Clutter;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AddClutter : BaseZoneGenerator
{
    public const int MaxChoicesPerClutterType = 3;
    public const float MaxSteepness = 15;
    public const float RandomClutterDensity = 0.00025f;

    protected IAddNearbyItemsHelper _addNearbyItemsHelper;

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
        if (zone == null || endx <= startx || endz <= startz)
        {
            return;
        }


        int dx = endx - startx;
        int dz = endz - startz;

        MyRandom rand = new MyRandom(zone.Seed % 2000000000 + 15434454);


        float clutterDensity = RandUtils.FloatRange(0.0f, 1.0f, rand) * RandomClutterDensity;

        if (_gameData.Get<ClutterTypeSettings>(_gs.ch).GetData() == null)
        {
            return;
        }

        int totalPlaced = 0;


        int size = Math.Max(zone.MaxX - zone.MinX, zone.MaxZ - zone.MinZ);

        int area = (zone.MaxX - zone.MinX) * (zone.MaxZ - zone.MinZ);

        int totalNumber = (int)(area * clutterDensity);

        int totalTries = 20 * totalNumber;
        for (long times = 0; times < totalTries; times++)
        {

            if (totalPlaced >= totalNumber)
            {
                break;
            }

            int x = RandUtils.IntRange(startx, endx, rand);
            int z = RandUtils.IntRange(startz, endz, rand);

            if (FlagUtils.MatchesAnyBits(_md.Flags[x, z], MapGenFlags.BelowWater))
            {
                continue;
            }

            if (x < 0 || x >= _mapProvider.GetMap().GetHwid() || z < 0 || z >= _mapProvider.GetMap().GetHhgt())
            {
                continue;
            }

            if (_zoneGenService.FindMapLocation(x, z, 5) != null)
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
            if (_md.Alphas[x, z, TerrainTexChannels.Road] > 0)
            {
                continue;
            }


            if (_terrainManager.GetSteepness(x, z) > MaxSteepness)
            {
                continue;
            }

            int currQuantityToPlace = 2 + RandUtils.IntRange(0, 1, rand);


            for (int i = 0; i < 5; i++)
            {
                if (rand.NextDouble() < 0.3f)
                {
                    currQuantityToPlace++;
                }
                else
                {
                    break;
                }
            }

            if (rand.NextDouble() < 0.1f)
            {
                currQuantityToPlace += RandUtils.IntRange(0, currQuantityToPlace / 2, rand);
            }

            int maxOffset = 1;

            if (currQuantityToPlace > 8)
            {
                maxOffset++;
            }

            List<Point2I> openPositions = new List<Point2I>();

            for (int xx = x - maxOffset; xx <= x + maxOffset; xx++)
            {
                if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                {
                    continue;
                }

                for (int zz = z - maxOffset; zz <= z + maxOffset; zz++)
                {
                    if (zz < 0 || zz >= _mapProvider.GetMap().GetHhgt())
                    {
                        continue;
                    }

                    if (_md.CellHasObject(xx, zz))
                    {
                        continue;
                    }
                    openPositions.Add(new Point2I(xx, zz));
                }
            }

            if (openPositions.Count < 1)
            {
                continue;
            }

            totalPlaced++;

            int totalClutterChoices = 0;
            Dictionary<ClutterType, int> clutterWeights = new Dictionary<ClutterType, int>();
            foreach (ClutterType ctype in _gameData.Get<ClutterTypeSettings>(_gs.ch).GetData())
            {
                if (ctype.NumChoices > 0)
                {
                    clutterWeights[ctype] = rand.Next() % 20 + 1;
                    totalClutterChoices += clutterWeights[ctype];
                }
            }

            if (totalClutterChoices < 1)
            {
                break;
            }

            for (int p = 0; p < currQuantityToPlace; p++)
            {

                if (openPositions.Count < 1)
                {
                    continue;
                }

                Point2I pos = openPositions[rand.Next() % openPositions.Count];
                int px = (int)(pos.X);
                int pz = (int)(pos.Z);
                openPositions.Remove(pos);

                int nearbyItemsCount = _addNearbyItemsHelper.GetNearbyItemsCount(maxOffset, rand);

                int clutterTypeChosen = rand.Next() % totalClutterChoices;

                ClutterType ctypeChosen = null;
                foreach (ClutterType ctype2 in clutterWeights.Keys)
                {
                    clutterTypeChosen -= clutterWeights[ctype2];
                    if (clutterTypeChosen < 0)
                    {
                        ctypeChosen = ctype2;
                        break;
                    }
                }

                if (ctypeChosen == null)
                {
                    continue;
                }

                _md.SetEntityData(px, pz, EntityTypes.Prop, ctypeChosen.IdKey);
            }
            int numToPlace = 4 + (currQuantityToPlace + 1) / 2;
            if (numToPlace < 3)
            {
                numToPlace = 3;
            }

            double rval = rand.NextDouble();
            if (rval <= 0.3f)
            {
                numToPlace = 0;
            }
            else if (rval <= 0.85f)
            {
                numToPlace /= 2;
            }

            float currMaxOffset = RandUtils.FloatRange(0.7f, 1.2f, rand);
            _addNearbyItemsHelper.AddItemsNear(rand, zoneType, zone, x, z, 0.9f, numToPlace, 1.0f, currMaxOffset);
        }
    }
}

