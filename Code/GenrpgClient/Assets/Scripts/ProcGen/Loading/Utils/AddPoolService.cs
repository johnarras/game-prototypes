using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.ProcGen.Settings.MapWater;
using OxDb.SharedGame.ProcGen.Settings.Trees;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.ProcGen.Loading.Utils
{
    public interface IAddPoolService : IInjectable
    {
        bool TryAddPool(WaterGenData genData);
    }

    public class AddPoolService : IAddPoolService
    {
        private IGameData _gameData = null;
        protected IMapProvider _mapProvider;
        protected IClientGameState _gs;
        protected IMapGenData _md;
        protected List<BushType> waterPlants = null;

        public bool TryAddPool(WaterGenData genData)
        {
            if (genData == null ||
                genData.x < MapConstants.TerrainPatchSize ||
                genData.z < MapConstants.TerrainPatchSize ||
                genData.x > _mapProvider.GetMap().GetHwid() - MapConstants.TerrainPatchSize ||
                genData.z > _mapProvider.GetMap().GetHhgt() - MapConstants.TerrainPatchSize)
            {
                return false;
            }

            if (waterPlants == null)
            {
                waterPlants = _gameData.Get<BushTypeSettings>(_gs.ch).GetData().Where(x => x.HasFlag(BushFlags.IsWaterItem)).ToList();
            }

            if (genData.stepSize < 1)
            {
                genData.stepSize = 1;
            }

            int nx = genData.x + 0;
            int nz = genData.z + 0;

            int cx = genData.x;
            int cz = genData.z;


            if (_md.CellHasObject(nx, nz))
            {
                bool foundOkPosition = false;
                for (int xx = nx - 1; xx <= nx + 1; xx++)
                {
                    for (int zz = nz - 1; zz <= nz + 1; zz++)
                    {
                        if (!_md.CellHasObject(xx, zz))
                        {
                            nz = zz;
                            nx = xx;
                            foundOkPosition = true;
                            break;
                        }
                    }
                    if (foundOkPosition)
                    {
                        break;
                    }
                }

                if (!foundOkPosition)
                {
                    return false;
                }

            }

            int xSizeMaxHeight = 0;
            int zSizeMaxHeight = 0;
            float maxHeightDiff = 0;

            int extraEdge = 4;

            float centerHeight = _md.Heights[cx, cz] * MapConstants.MapHeight;

            float minHeightTotal = centerHeight + 3;





            for (int xsize = genData.minXSize; xsize <= genData.maxXSize; xsize += genData.stepSize)
            {
                for (int zsize = genData.minZSize; zsize <= genData.maxZSize; zsize += genData.stepSize)
                {


                    bool nearWater = false;
                    float minHeightAroundEdges = MapConstants.MapHeight;

                    float minHeightAnywhere = MapConstants.MapHeight;
                    for (int xx = cx - xsize - extraEdge; xx <= cx + xsize + extraEdge; xx++)
                    {
                        if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid() || nearWater)
                        {
                            minHeightAroundEdges = 0;
                            minHeightAnywhere = 0;
                            continue;
                        }

                        float dx = (xx - cx) * 1.0f / (xsize);
                        float ddx = dx * dx;
                        for (int zz = cz - zsize - extraEdge; zz <= cz + zsize + extraEdge; zz++)
                        {

                            if (zz < 0 || zz >= _mapProvider.GetMap().GetHwid())
                            {
                                minHeightAroundEdges = 0;
                                minHeightAnywhere = 0;
                                continue;
                            }

                            if (FlagUtils.MatchesAnyBits(_md.Flags[xx, zz], MapGenFlags.NearWater))
                            {
                                nearWater = true;
                                break;
                            }

                            float dy = (zz - cz) * 1.0f / (zsize);
                            float ddy = dy * dy;


                            float currHeight = _md.Heights[xx, zz] * MapConstants.MapHeight;
                            if (currHeight < minHeightAnywhere)
                            {
                                minHeightAnywhere = currHeight;
                            }
                            if (ddx + ddy >= 1)
                            {
                                if (currHeight < minHeightAroundEdges)
                                {
                                    minHeightAroundEdges = currHeight;
                                }
                            }
                        }
                    }


                    if (genData.maxHeight > 0 && minHeightAroundEdges > genData.maxHeight)
                    {
                        minHeightAroundEdges = genData.maxHeight;
                    }


                    if (minHeightAroundEdges < minHeightAnywhere + 2)
                    {
                        continue;
                    }



                    int heightDiff = (int)((minHeightAroundEdges - MapConstants.MinLandHeight));



                    if (heightDiff > maxHeightDiff)
                    {
                        xSizeMaxHeight = zsize;
                        zSizeMaxHeight = xsize;
                        maxHeightDiff = heightDiff;
                    }
                }
            }

            int nearWaterRad = 2;

            if (xSizeMaxHeight > 0 && zSizeMaxHeight > 0 && maxHeightDiff > 1)
            {
                float waterHeight = (MapConstants.MinLandHeight + (int)maxHeightDiff - 0.5f) / MapConstants.MapHeight;
                float maxPlantHeight = waterHeight + 0.5f / MapConstants.MapHeight;

                MyRandom rand = new MyRandom(genData.x * 31 + genData.z * 71);

                float plantChance = RandUtils.FloatRange(0, 1, rand);

                for (int xx = cx - xSizeMaxHeight - extraEdge; xx <= cx + xSizeMaxHeight + extraEdge; xx++)
                {
                    if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                    {
                        continue;
                    }
                    for (int zz = cz - zSizeMaxHeight - extraEdge; zz <= cz + zSizeMaxHeight + extraEdge; zz++)
                    {
                        if (zz < 0 || zz >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }

                        _md.Flags[xx, zz] |= MapGenFlags.NearWater;
                        if (_md.Heights[xx, zz] < waterHeight)
                        {
                            _md.Flags[xx, zz] |= MapGenFlags.BelowWater;
                        }


                        int tx = xx + 0 * (xx / (MapConstants.TerrainPatchSize - 1));
                        int ty = zz + 0 * (zz / (MapConstants.TerrainPatchSize - 1));

                        long entityTypeId = _md.EntityTypeIds[tx, ty];

                        if (entityTypeId != EntityTypes.Bridge)
                        {
                            if (_md.Heights[xx, zz] < waterHeight)
                            {
                                //_md.mapObjects[tx,ty] = 0;
                            }
                            else
                            {

                                float dxpct = 1.0f * (cx - xx) / xSizeMaxHeight;
                                float dypct = 1.0f * (cz - zz) / zSizeMaxHeight;

                                float dpct = dxpct * dxpct + dypct * dypct;
                                if (dpct <= 1.0f)
                                {
                                    int ux = tx + 0 * (xx / (MapConstants.TerrainPatchSize - 1));
                                    int uy = ty + 0 * (zz / (MapConstants.TerrainPatchSize - 1));
                                    if (!_md.CellHasObject(ux, uy) && waterPlants.Count > 0 &&
                                        _md.Heights[xx, zz] < maxPlantHeight && rand.NextDouble() < plantChance)
                                    {
                                        bool nearRealWater = false;
                                        for (int x1 = xx - nearWaterRad; x1 <= xx + nearWaterRad; x1++)
                                        {
                                            if (nearRealWater)
                                            {
                                                break;
                                            }

                                            if (x1 < 0 || x1 >= _mapProvider.GetMap().GetHwid())
                                            {
                                                continue;
                                            }

                                            for (int z1 = zz - nearWaterRad; z1 <= zz + nearWaterRad; z1++)
                                            {
                                                if (z1 < 0 || z1 >= _mapProvider.GetMap().GetHhgt())
                                                {
                                                    continue;
                                                }

                                                if (_md.Heights[x1, z1] < waterHeight)
                                                {
                                                    nearRealWater = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (nearRealWater)
                                        {
                                            BushType plantChosen = waterPlants[rand.Next() % waterPlants.Count];
                                            _md.SetEntityData(ux, uy, EntityTypes.Bush, plantChosen.IdKey);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _md.SetEntityData(nx, nz, EntityTypes.Water, 0);

                _md.ExtendedObjects[nx, nz] = new ExtendedWorldObjectData()
                {
                    X = nx,
                    Z = nz,
                    XSize = (int)(xSizeMaxHeight),
                    ZSize = (int)(zSizeMaxHeight),
                    Height = (ushort)maxHeightDiff,
                    EntityTypeId = EntityTypes.Water,
                    EntityId = 0,
                };

                return true;
            }
            return false;
        }

    }
}


