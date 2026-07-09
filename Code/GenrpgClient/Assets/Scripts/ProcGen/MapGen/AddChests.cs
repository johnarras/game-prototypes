using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.GroundObjects.Settings;
using OxDb.SharedGame.Spawns.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class AddChests : BaseZoneGenerator
{
    public const float MaxSteepness = 25;
    public const float ChestChance = 0.1f;

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        MyRandom placeRand = new MyRandom(_mapProvider.GetMap().Seed % 102839484);
        MyRandom choiceRand = new MyRandom(_mapProvider.GetMap().Seed % 329377421);

        int skipSize = 40;

        List<GroundObjType> chests = _gameData.Get<GroundObjTypeSettings>(_gs.ch).GetData().Where(x => x.GroupId == GroundObjType.ChestGroup).ToList();

        if (chests == null || chests.Count < 1)
        {
            return;
        }

        int totalWeight = chests.Sum(x => x.SpawnWeight);

        if (totalWeight < 1)
        {
            return;
        }

        for (int x = MapConstants.TerrainPatchSize; x < _mapProvider.GetMap().GetHwid() - MapConstants.TerrainPatchSize; x += skipSize)
        {
            for (int z = MapConstants.TerrainPatchSize; z < _mapProvider.GetMap().GetHhgt() - MapConstants.TerrainPatchSize; z += skipSize)
            {
                if (FlagUtils.MatchesAnyBits(_md.Flags[x, z], MapGenFlags.BelowWater | MapGenFlags.IsLocation))
                {
                    continue;
                }
                if (placeRand.NextDouble() > ChestChance)
                {
                    continue;
                }

                GroundObjType chosenObj = null;

                int chestChoice = choiceRand.Next() % totalWeight;

                foreach (GroundObjType chest in chests)
                {
                    chestChoice -= chest.SpawnWeight;
                    if (chestChoice <= 0)
                    {
                        chosenObj = chest;
                        break;
                    }
                }

                if (chosenObj == null)
                {
                    continue;
                }

                int nearbyRadius = 3;

                for (int times = 0; times < 20; times++)
                {

                    int cx = x + RandUtils.IntRange(-skipSize / 3, skipSize / 3, placeRand);
                    int cz = z + RandUtils.IntRange(-skipSize / 3, skipSize / 3, placeRand);

                    bool haveNearbyItem = false;


                    for (int xx = cx - nearbyRadius; xx <= cx + nearbyRadius; xx++)
                    {
                        for (int zz = cz - nearbyRadius; zz <= cz + nearbyRadius; zz++)
                        {

                            if (_md.CellHasObject(xx, zz))
                            {
                                haveNearbyItem = true;
                                break;
                            }
                        }
                        if (haveNearbyItem)
                        {
                            break;
                        }
                    }

                    if (haveNearbyItem)
                    {
                        continue;
                    }


                    int tx = cx - cx / (MapConstants.TerrainPatchSize - 1);
                    int tz = cz - cz / (MapConstants.TerrainPatchSize - 1);

                    if (_zoneGenService.FindMapLocation(tx, tz, 10) != null)
                    {
                        continue;
                    }


                    if (_md.RoadDistances[cx, cz] < 30)
                    {
                        continue;
                    }


                    if (_terrainManager.GetSteepness(cx, cz) > MaxSteepness)
                    {
                        continue;
                    }

                    InitSpawnData initData = new InitSpawnData()
                    {
                        EntityTypeId = EntityTypes.GroundObject,
                        EntityId = chosenObj.IdKey,
                        SpawnX = cz,
                        SpawnZ = cx,
                        ZoneId = _md.MapZoneIds[cx, cz],
                        ZoneOverridePercent = (int)(_md.OverrideZoneScales[cx, cz] * MapConstants.OverrideZoneScaleMax),
                    };


                    _mapProvider.GetSpawns().AddSpawn(initData);

                    break;
                }
            }
        }
    }
}

