using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Buildings.Settings;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.Pathfinding.Constants;
using OxDb.SharedGame.ProcGen.Settings.Trees;
using OxDb.SharedGame.Spawns.WorldData;
using System;
using System.Threading;
using UnityEngine;

public class CreatePathfindingData : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        try
        {
            int blockSize = PathfindingConstants.BlockSize;
            int pxsize = _mapProvider.GetMap().GetHwid() / blockSize;
            int pzsize = _mapProvider.GetMap().GetHhgt() / blockSize;

            bool[,] blockedCells = new bool[pxsize, pzsize];
            bool[,] nearBlockedCells = new bool[pxsize, pzsize];

            for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
            {
                if (x < MapConstants.MapEdgeSize || x >= _mapProvider.GetMap().GetHwid() - MapConstants.MapEdgeSize - 1)
                {
                    continue;
                }
                int px = (x + 1) / blockSize;
                if (px < 0 || px >= pxsize)
                {
                    continue;
                }
                for (int z = 0; z < _mapProvider.GetMap().GetHhgt(); z++)
                {
                    if (z < MapConstants.MapEdgeSize || z >= _mapProvider.GetMap().GetHhgt() - MapConstants.MapEdgeSize - 1)
                    {
                        continue;
                    }
                    int pz = (z + 1) / blockSize;
                    if (pz < 0 || pz >= pzsize)
                    {
                        continue;
                    }

                    float steepness = _terrainManager.GetSteepness(x, z);

                    if (steepness > PathfindingConstants.MaxSteepness)
                    {
                        blockedCells[px, pz] = true;
                    }

                    if (_md.CellHasObject(x, z))
                    {
                        if (_md.EntityTypeIds[x, z] == EntityTypes.Tree)
                        {
                            TreeType ttype = _gameData.Get<TreeTypeSettings>(_gs.ch).Get(_md.EntityIds[x, z]);

                            if (ttype != null && !ttype.HasFlag(TreeFlags.IsBush))
                            {
                                blockedCells[pz, px] = true;
                            }

                            else
                            {
                                blockedCells[pz, px] = false;
                            }
                        }
                    }
                }
            }

            // Now block out all building spawns.
            foreach (MapSpawn spawn in _mapProvider.GetSpawns().Data)
            {
                if (spawn.EntityTypeId == EntityTypes.Building)
                {
                    BuildingType btype = _gameData.Get<BuildingSettings>(null).Get(spawn.EntityId);
                    if (btype != null)
                    {
                        int buildingRadius = 4;
                        for (int x = (int)spawn.X - buildingRadius + 1; x <= spawn.X + buildingRadius; x++)
                        {
                            int px = x / blockSize;
                            if (px < 0 || px >= pxsize)
                            {
                                continue;
                            }
                            for (int z = (int)spawn.Z - buildingRadius + 1; z <= spawn.Z + buildingRadius; z++)
                            {
                                int pz = z / blockSize;
                                if (pz < 0 || pz >= pzsize)
                                {
                                    continue;
                                }

                                blockedCells[px, pz] = true;
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < pxsize; x++)
            {
                for (int z = 0; z < pzsize; z++)
                {
                    if (blockedCells[x, z]
                        //|| (x > 0 && blockedCells[x - 1, z]) 
                        // || (x < pxsize-1 && blockedCells[x + 1, z]) 
                        //|| (z > 0 && blockedCells[x, z - 1])
                        // || (z < pzsize - 1  && blockedCells[x, z + 1])
                        )
                    {
                        nearBlockedCells[x, z] = true;
                    }
                }
            }

            byte[] output = _pathfindingService.ConvertGridToBytes(nearBlockedCells);

            int startLength = output.Length;

            output = CompressionUtils.CompressBytes(output);

            int endLength = output.Length;

            string filename = MapUtils.GetMapObjectFilename(PathfindingConstants.Filename, _mapProvider.GetMap().Id, _mapProvider.GetMap().MapVersion);
            await _clientRepoService.SaveBytes(filename, output);
        }
        catch (Exception e)
        {
            _logService.Exception(e, "Pathfinding");
        }
    }
}


