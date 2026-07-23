using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.ProcGen.Entities;
using OxDb.SharedGame.ProcGen.Settings.Trees;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;

public interface IAddNearbyItemsHelper : IInjectable
{
    int GetNearbyItemsCount(int radius, MyRandom rand);
    void AddItemsNear(IRandom rand, ZoneType zoneType, Zone zone, int x, int z, double placeChance, int maxPlaceQuantity, float minOffset, float maxOffset, bool canPlaceTrees = true);
}



/// <summary>
/// Add items nearby a zoneTree or a rock or something of that sort.
/// </summary>
public class AddNearbyItemsHelper : IAddNearbyItemsHelper
{
    private IClientGameState _gs;
    private IGameData _gameData;
    private IMapProvider _mapProvider;
    private IMapGenData _mapGenData;
    private IMapTerrainManager _terrainManager;
    public void AddItemsNear(IRandom rand, ZoneType zoneType, Zone zone, int x, int z, double placeChance, int maxPlaceQuantity, float minOffset, float maxOffset, bool canPlaceTrees = true)
    {

        float posHeight = _terrainManager.GetInterpolatedHeight(z, x);

        if (posHeight < MapConstants.OceanHeight)
        {
            return;
        }

        if (rand.NextDouble() > placeChance)
        {
            return;
        }

        int maxNumPlants = maxPlaceQuantity;

        int bushesToAdd = RandUtils.IntRange(maxNumPlants / 2, maxNumPlants, rand);

        if (rand.NextDouble() < 0.3f)
        {
            bushesToAdd += RandUtils.IntRange(1, maxNumPlants, rand);
        }

        int treesToAdd = bushesToAdd / 10;

        if (treesToAdd == 0 && rand.Next() < 0.10f)
        {
            treesToAdd++;
        }

        if (treesToAdd > 2)
        {
            treesToAdd = 2;
        }

        GenZone genZone = _mapGenData.GetGenZone(zone.IdKey);


        List<WeightedEntity> treeList = genZone.GetPropsOfType(EntityTypes.Tree);
        List<WeightedEntity> bushList = genZone.GetPropsOfType(EntityTypes.Bush);


        if (treeList.Count < 1 || bushList.Count < 1)
        {
            return;
        }

        foreach (WeightedEntity zoneTree in treeList)
        {
            if (zoneTree.Weight <= 0)
            {
                continue;
            }
            TreeType tt = _gameData.Get<TreeTypeSettings>(_gs.ch).Get(zoneTree.EntityId);
            if (tt == null || tt.Name == null)
            {
                continue;
            }
            treeList.Add(zoneTree);

        }

        foreach (WeightedEntity zoneBush in bushList)
        {
            if (zoneBush.Weight <= 0)
            {
                continue;
            }
            BushType btype = _gameData.Get<BushTypeSettings>(_gs.ch).Get(zoneBush.EntityId);
            if (btype == null || btype.Name == null)
            {
                continue;
            }
            if (btype.HasFlag(BushFlags.IsWaterItem))
            {
                continue;
            }

            bushList.Add(zoneBush);
        }




        if (!canPlaceTrees)
        {
            treeList = new List<WeightedEntity>();
        }

        if (minOffset < 0.99f)
        {
            minOffset = 0.99f;
        }

        float maxTreeOffset = Math.Max(2, maxOffset * 2 / 3);
        float maxBushOffset = Math.Max(2, maxOffset + 1);

        maxTreeOffset = Math.Max(minOffset + 2, maxTreeOffset);
        maxBushOffset = Math.Max(minOffset + 1, maxBushOffset);


        for (int plantTimes = 0; plantTimes < 2; plantTimes++)
        {
            int numToPlace = treesToAdd;
            double offset = maxTreeOffset;
            List<IWeightedItemId> itemList = treeList.Cast<IWeightedItemId>().ToList();
            long entityTypeId = EntityTypes.Tree;

            if (plantTimes == 1)
            {
                numToPlace = bushesToAdd;
                offset = maxBushOffset;
                itemList = bushList.Cast<IWeightedItemId>().ToList();
                entityTypeId = EntityTypes.Bush;
            }

            if (itemList.Count < 1)
            {
                continue;
            }

            int numPlaced = 0;
            for (int tries = 0; tries < numToPlace * 30 && numPlaced < numToPlace; tries++)
            {
                int plantx = (int)(x + RandUtils.DeltaRange(offset, rand) + 0.5f);
                int planty = (int)(z + RandUtils.DeltaRange(offset, rand) + 0.5f);

                int pdx = plantx - x;
                int pdy = planty - z;


                float dist = (float)Math.Sqrt(pdx * pdx + pdy * pdy);
                if (dist < minOffset)
                {
                    continue;
                }
                if (dist > offset)
                {
                    continue;
                }


                int ipplantx = (int)(plantx); //+ (int)(plantx / (MapConstants.TerrainPatchSize - 1));
                int ipplanty = (int)(planty); //+ (int)(planty / (MapConstants.TerrainPatchSize - 1));

                if (ipplantx < 0 || ipplantx >= _mapProvider.GetMap().GetHwid() || ipplanty <= 0 || ipplanty >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }


                if (_mapGenData.RoadDistances[ipplantx, ipplanty] < 3)
                {
                    continue;
                }

                IWeightedItemId item = itemList[rand.Next() % itemList.Count];

                _mapGenData.SetEntityData(ipplantx, ipplanty, entityTypeId, item.GetId());
            }
        }
    }

    public int GetNearbyItemsCount(int radius, MyRandom rand)
    {
        int nearbyItemsCount = 1;

        for (int newItemTimes = 0; newItemTimes < 3; newItemTimes++)
        {
            if (rand.Next() % 5 > newItemTimes)
            {
                nearbyItemsCount += RandUtils.IntRange(0, 2, rand);
            }
        }

        if (nearbyItemsCount < 1)
        {
            nearbyItemsCount = 1;
        }

        for (int rad = 1; rad <= radius; rad++)
        {
            nearbyItemsCount = nearbyItemsCount * 3 / 2;
        }

        nearbyItemsCount = RandUtils.IntRange(nearbyItemsCount * 3 / 4, nearbyItemsCount * 5 / 4, rand);


        return nearbyItemsCount;
    }
}


