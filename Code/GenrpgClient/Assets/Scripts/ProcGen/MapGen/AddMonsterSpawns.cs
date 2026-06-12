

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
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.XMin, zone.ZMin, zone.XMax, zone.ZMax);
        }
    }
    public void GenerateOne(Zone zone, ZoneType zoneType, int startx, int starty, int endx, int endy)
    {
        if (zone == null || zoneType == null || startx >= endx || starty >= endy ||
            _mapProvider.GetMap() == null)
        {
            return;
        }

        startx = MathUtil.Clamp(MapConstants.MapEdgeSize, startx, _mapProvider.GetMap().GetHwid() - MapConstants.MapEdgeSize);
        starty = MathUtil.Clamp(MapConstants.MapEdgeSize, starty, _mapProvider.GetMap().GetHhgt() - MapConstants.MapEdgeSize);


        endx = MathUtil.Clamp(MapConstants.MapEdgeSize, endx, _mapProvider.GetMap().GetHwid() - MapConstants.MapEdgeSize);
        endy = MathUtil.Clamp(MapConstants.MapEdgeSize, endy, _mapProvider.GetMap().GetHhgt() - MapConstants.MapEdgeSize);

        MyRandom rand = new MyRandom(zone.Seed + 1);

        int minZoneDist = 4;
        int zoneCheckSkip = 3;

        int offsetSize = MapConstants.MonsterSpawnSkipSize / 4;

        for (int x = startx; x <= endx; x += MapConstants.MonsterSpawnSkipSize)
        {
            for (int y = starty; y <= endy; y += MapConstants.MonsterSpawnSkipSize)
            {
                int cx = x + RandUtils.IntRange(-offsetSize, offsetSize, rand);
                int cy = y + RandUtils.IntRange(-offsetSize, offsetSize, rand);

                if (cx < 0 || cy < 0 || cx >= _mapProvider.GetMap().GetHwid() || cy >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }

                if (cx < 0 || cy < 0 || cx >= _mapProvider.GetMap().GetHwid() || cy >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }
                if (FlagUtils.MatchesAnyBits(_md.Flags[cx, cy], MapGenFlags.BelowWater))
                {
                    continue;
                }

                if (_md.CellHasObject(cx, cy))
                {
                    continue;
                }

                if (_md.MaintainHeights[cx, cy] >= 1.0f)
                {
                    continue;
                }

                if (_md.MountainDistPercent[cx, cy] < 0.1f)
                {
                    continue;
                }

                if (_terrainManager.GetSteepness(cx, cy) > PathfindingConstants.MaxSteepness)
                {
                    continue;
                }

                if (_md.Heights[cx, cy] <= (MapConstants.MinLandHeight * 7 / 10) / MapConstants.MapHeight)
                {
                    continue;
                }


                if (_md.BridgeDistances[cx, cy] < 20)
                {
                    continue;
                }

                if (_zoneGenService.FindMapLocation(cx, cy, 15) != null)
                {
                    continue;
                }


                if (_md.RoadDistances[cx, cy] < 4)
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
                    for (int yy = cy - minZoneDist; yy <= cy + minZoneDist; yy += zoneCheckSkip)
                    {
                        if (yy < 0 || yy >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }
                        if (_md.MapZoneIds[xx, yy] != zone.IdKey)
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
                if (_md.SubZoneIds[cx, cy] > 0)
                {
                    zoneId = _md.SubZoneIds[cx, cy];
                }

                InitSpawnData initData = new InitSpawnData()
                {
                    EntityTypeId = EntityTypes.ZoneUnit,
                    EntityId = zone.IdKey,
                    SpawnX = cy,
                    SpawnZ = cx,
                    ZoneId = _md.MapZoneIds[cx, cy],
                    ZoneOverridePercent = (int)(_md.OverrideZoneScales[cx, cy] * MapConstants.OverrideZoneScaleMax),
                };


                _mapProvider.GetSpawns().AddSpawn(initData);

            }
        }
    }
}



