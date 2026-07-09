

using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Pathfinding.Constants;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Threading;
using UnityEngine;

public class AddMonsterSpawns : BaseZoneGenerator
{

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
        if (zone == null || zoneType == null || startx >= endx || startz >= endz ||
            _mapProvider.GetMap() == null)
        {
            return;
        }

        startx = MathUtil.Clamp(MapConstants.MapEdgeSize, startx, _mapProvider.GetMap().GetHwid() - MapConstants.MapEdgeSize);
        startz = MathUtil.Clamp(MapConstants.MapEdgeSize, startz, _mapProvider.GetMap().GetHhgt() - MapConstants.MapEdgeSize);

        endx = MathUtil.Clamp(MapConstants.MapEdgeSize, endx, _mapProvider.GetMap().GetHwid() - MapConstants.MapEdgeSize);
        endz = MathUtil.Clamp(MapConstants.MapEdgeSize, endz, _mapProvider.GetMap().GetHhgt() - MapConstants.MapEdgeSize);

        MyRandom rand = new MyRandom(zone.Seed + 1);

        int minZoneDist = 4;
        int zoneCheckSkip = 3;

        int offsetSize = MapConstants.MonsterSpawnSkipSize / 4;

        for (int x = startx; x <= endx; x += MapConstants.MonsterSpawnSkipSize)
        {
            for (int z = startz; z <= endz; z += MapConstants.MonsterSpawnSkipSize)
            {
                int cx = x + RandUtils.IntRange(-offsetSize, offsetSize, rand);
                int cz = z + RandUtils.IntRange(-offsetSize, offsetSize, rand);

                if (cx < 0 || cz < 0 || cx >= _mapProvider.GetMap().GetHwid() || cz >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }

                if (cx < 0 || cz < 0 || cx >= _mapProvider.GetMap().GetHwid() || cz >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }
                if (FlagUtils.MatchesAnyBits(_md.Flags[cx, cz], MapGenFlags.BelowWater))
                {
                    continue;
                }

                if (_md.CellHasObject(cx, cz))
                {
                    continue;
                }

                if (_md.MaintainHeights[cx, cz] >= 1.0f)
                {
                    continue;
                }

                if (_md.MountainDistPercent[cx, cz] < 0.1f)
                {
                    continue;
                }

                if (_terrainManager.GetSteepness(cx, cz) > PathfindingConstants.MaxSteepness)
                {
                    continue;
                }

                if (_md.Heights[cx, cz] <= (MapConstants.MinLandHeight * 7 / 10) / MapConstants.MapHeight)
                {
                    continue;
                }


                if (_md.BridgeDistances[cx, cz] < 20)
                {
                    continue;
                }

                if (_zoneGenService.FindMapLocation(cx, cz, 15) != null)
                {
                    continue;
                }

                if (_md.RoadDistances[cx, cz] < 4)
                {
                    continue;
                }
                bool nearAnotherZone = false;
                for (int xx = cx - minZoneDist; xx <= cx + minZoneDist; xx += zoneCheckSkip)
                {
                    if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                    {
                        continue;
                    }
                    for (int zz = cz - minZoneDist; zz <= cz + minZoneDist; zz += zoneCheckSkip)
                    {
                        if (zz < 0 || zz >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }
                        if (_md.MapZoneIds[xx, zz] != zone.IdKey)
                        {
                            nearAnotherZone = true;
                            break;
                        }
                    }

                    if (nearAnotherZone)
                    {
                        break;
                    }
                }

                if (nearAnotherZone)
                {
                    continue;
                }

                long zoneId = zone.IdKey;
                if (_md.SubZoneIds[cx, cz] > 0)
                {
                    zoneId = _md.SubZoneIds[cx, cz];
                }

                InitSpawnData initData = new InitSpawnData()
                {
                    EntityTypeId = EntityTypes.ZoneUnit,
                    EntityId = zone.IdKey,
                    SpawnX = cz,
                    SpawnZ = cx,
                    ZoneId = _md.MapZoneIds[cx, cz],
                    ZoneOverridePercent = (int)(_md.OverrideZoneScales[cx, cz] * MapConstants.OverrideZoneScaleMax),
                };

                _mapProvider.GetSpawns().AddSpawn(initData);

            }
        }
    }
}



