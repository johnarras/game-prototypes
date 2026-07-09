using Assets.Scripts.MapTerrain;
using OxDb.SharedCore.MapServer.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class SetupTerrainPatches : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        for (int px = 0; px < MapConstants.MaxTerrainGridSize; px++)
        {
            for (int pz = 0; pz < MapConstants.MaxTerrainGridSize; pz++)
            {
                if (_terrainManager.GetTerrainPatch(px, pz, false) == null)
                {
                    _terrainManager.SetTerrainPatchAtGridLocation(px, pz, null, null);
                }

                UnityEngine.TerrainData tdata = _terrainManager.GetTerrainData(px, pz);
                if (tdata == null)
                {
                    continue;
                }

                TerrainPatchData patch = _terrainManager.GetTerrainPatch(px, pz, false);
                int sx = px * (MapConstants.TerrainPatchSize - 1);
                int sz = pz * (MapConstants.TerrainPatchSize - 1);

                Dictionary<int, int> baseZoneIdCounts = new Dictionary<int, int>();

                for (int z = sz; z <= sz + MapConstants.TerrainPatchSize && z < _mapProvider.GetMap().GetHhgt(); z++)
                {
                    for (int x = sx; x <= sx + MapConstants.TerrainPatchSize && x < _mapProvider.GetMap().GetHwid(); x++)
                    {
                        int zoneId = _md.MapZoneIds[z, x];
                        if (zoneId < SharedMapConstants.MapZoneStartId)
                        {
                            _logService.Info("Missing zoneId at " + x + " " + z);
                        }
                        else if (!patch.FullZoneIdList.Contains(zoneId))
                        {
                            patch.FullZoneIdList.Add(zoneId);
                            if (!baseZoneIdCounts.ContainsKey(zoneId))
                            {
                                baseZoneIdCounts[zoneId] = 0;
                            }
                            baseZoneIdCounts[zoneId]++;
                        }
                        int baseZoneId = _md.SubZoneIds[z, x];

                        if (baseZoneId >= SharedMapConstants.MinBaseZoneId && !patch.FullZoneIdList.Contains(baseZoneId))
                        {
                            patch.FullZoneIdList.Add(baseZoneId);
                        }

                        if (z - sz < MapConstants.TerrainPatchSize && x - sx < MapConstants.TerrainPatchSize)
                        {
                            patch.mainZoneIds[z - sz, x - sx] = (byte)_md.MapZoneIds[z, x];
                            patch.subZoneIds[z - sz, x - sx] = (byte)_md.SubZoneIds[z, x];
                        }
                    }
                }
                ;

                if (baseZoneIdCounts.Values.Count > 0)
                {
                    int maxZoneIdCount = baseZoneIdCounts.Values.Max();

                    int biggestZoneId = -1;

                    foreach (int zid in baseZoneIdCounts.Keys)
                    {
                        if (baseZoneIdCounts[zid] == maxZoneIdCount)
                        {
                            biggestZoneId = zid;
                            break;
                        }
                    }

                    // Place this first so it's the only thing we look at for the purposes of grass.
                    patch.FullZoneIdList.Remove(biggestZoneId);
                    patch.FullZoneIdList.Insert(0, biggestZoneId);
                }
            }
        }
    }
}


