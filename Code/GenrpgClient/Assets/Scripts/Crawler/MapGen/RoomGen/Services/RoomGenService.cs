using OxDb.Client.Crawler.MapGen.Helpers;
using OxDb.Client.Crawler.MapGen.RoomGen.Entities;
using OxDb.Client.Crawler.MapGen.RoomGen.Helpers;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.MapGen.Settings;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.ProcGen.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.MapGen.RoomGen.Services
{

    public class RoomGenArgs
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinZ { get; set; }
        public int MaxZ { get; set; }

        public bool UseNoise { get; set; }
        public bool IsSymmetric { get; set; }
    }


    public interface IRoomGenService : IInitializable
    {
        ValueTask GenerateRoom(SampledPoint centerPoint, CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs);
    }

    public class RoomGenService : IRoomGenService
    {
        private IGameData _gameData = null;
        private IClientGameState _gs = null;


        private SetupDictionaryContainer<long, IEdgeGenHelper> _edgeGenHelpers = new SetupDictionaryContainer<long, IEdgeGenHelper>();

        public async Task Initialize(CancellationToken token)
        {

            await Task.CompletedTask;
        }
        public async ValueTask GenerateRoom(SampledPoint center, CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs)
        {
            IRandom rand = new MyRandom(levelArgs.Rand.Next());

            CrawlerMap map = levelArgs.Map;

            levelArgs.RoomCenters.Add(center);

            RoomGenSettings roomGenSettings = _gameData.Get<RoomGenSettings>(_gs.ch);
            RoomEdgeTypeSettings roomTypeSettings = _gameData.Get<RoomEdgeTypeSettings>(_gs.ch);

            int xsize = RandUtils.IntRange(roomGenSettings.MinSize, roomGenSettings.MaxSize, rand);

            int zsize = RandUtils.IntRange(roomGenSettings.MinSize, roomGenSettings.MaxSize, rand);

            if (rand.NextDouble() < roomGenSettings.SizeIncreaseChance && !map.IsOutdoorDungeon())
            {
                if (rand.NextDouble() < 0.5)
                {
                    xsize += RandUtils.IntRange(roomGenSettings.MinSize, roomGenSettings.MaxSize, rand);
                    if (rand.NextDouble() < roomGenSettings.SizeIncreaseChance)
                    {
                        zsize += RandUtils.IntRange(roomGenSettings.MinSize, roomGenSettings.MaxSize, rand);
                    }
                }
                else
                {
                    zsize += RandUtils.IntRange(roomGenSettings.MinSize, roomGenSettings.MaxSize, rand);
                    if (rand.NextDouble() < roomGenSettings.SizeIncreaseChance)
                    {
                        xsize += RandUtils.IntRange(roomGenSettings.MinSize, roomGenSettings.MaxSize, rand);
                    }
                }
            }


            if (rand.NextDouble() < roomGenSettings.SquareRoomChance)
            {
                zsize = xsize;
            }

#if UNITY_EDITOR
            if (BaseEdgeGenHelper.ForcedRoomEdgeType > 0)
            {
                xsize = 7;
                zsize = 7;
            }
#endif
            int edgeSize = (map.IsOutdoorDungeon() ? 3 : 1);

            int xmin = MathUtil.Clamp(edgeSize, center.X - xsize / 2, map.Width - edgeSize-1);
            int xmax = MathUtil.Clamp(edgeSize, center.X + (xsize + 1) / 2, map.Width - edgeSize - 1);
            int zmin = MathUtil.Clamp(edgeSize, center.Z - zsize / 2, map.Height - edgeSize - 1);
            int zmax = MathUtil.Clamp(edgeSize, center.Z + (zsize + 1) / 2, map.Height - edgeSize - 1);

            bool xminIsOk = true;
            bool xmaxIsOk = true;
            bool zminIsOk = true;
            bool zmaxIsOk = true;

            for (int x = xmin; x <= xmax; x++)
            {
                for (int z = zmin; z <= zmax; z++)
                {
                    if (levelArgs.AdjacentRoomIds[x, z] == 0)
                    {
                        levelArgs.RoomIds[x, z] = center.Index;
                        levelArgs.AdjacentRoomIds[x, z] = center.Index;
                        map.Set(x, z, CellIndex.Terrain, genData.ZoneType.IdKey);
                    }
                    else
                    {
                        if (x == xmin)
                        {
                            xminIsOk = false;
                        }
                        if (x == xmax)
                        {
                            xmaxIsOk = false;
                        }
                        if (z == zmin)
                        {
                            zminIsOk = false;
                        }
                        if (z == zmax)
                        {
                            zmaxIsOk = false;
                        }
                    }
                }
            }

            for (int x = xmin - 1; x <= xmax + 1; x++)
            {
                for (int z = zmin - 1; z <= zmax + 1; z++)
                {

                    if ((x != xmin - 1 || x != xmax + 1) && (z != zmin - 1 || z != zmax + 1))
                    {
                        if (levelArgs.AdjacentRoomIds[x, z] == 0)
                        {
                            levelArgs.AdjacentRoomIds[x, z] = center.Index;
                        }
                    }
                }
            }

            if (map.IsOutdoorDungeon())
            {
                return;
            }

            EdgePattern edgePattern = RandUtils.GetRandomElement(roomTypeSettings.EdgePatterns, levelArgs.Rand);

#if UNITY_EDITOR
            if (BaseEdgeGenHelper.ForcedRoomEdgeType > 0)
            {
                edgePattern = roomTypeSettings.EdgePatterns.FirstOrDefault(x => x.Quantity == 4);
            }
#endif

            if (edgePattern.Quantity < 1)
            {
                return;
            }

            bool edgesAreSymmetric = levelArgs.Rand.NextDouble() < edgePattern.SymmetricChance;
            edgesAreSymmetric = false;

            List<RoomEdgeGenArgs> allEdgeArgsList = new List<RoomEdgeGenArgs>();

            List<RoomEdgeGenArgs> upDownList = new List<RoomEdgeGenArgs>();

            List<RoomEdgeGenArgs> leftRightList = new List<RoomEdgeGenArgs>();

            CornerPositions corners = new CornerPositions(xmin, zmin, xmax, zmax);

            if (zminIsOk)
            {
                upDownList.Add(new RoomEdgeGenArgs(0, -1, center.Index, corners));
            }
            if (zmaxIsOk)
            {
                upDownList.Add(new RoomEdgeGenArgs(0, 1, center.Index, corners));
            }

            if (xminIsOk)
            {
                leftRightList.Add(new RoomEdgeGenArgs(-1, 0, center.Index, corners));
            }
            if (xmaxIsOk)
            {
                leftRightList.Add(new RoomEdgeGenArgs(1, 0, center.Index, corners));
            }

            allEdgeArgsList = upDownList.Concat(leftRightList).ToList();

            List<RoomEdgeType> randomizedEdgeTypes = new List<RoomEdgeType>();

            List<RoomEdgeType> remainingEdgeTypes = roomTypeSettings.GetData().ToList();

            while (remainingEdgeTypes.Count > 0)
            {
                RoomEdgeType nextType = RandUtils.GetRandomElement(remainingEdgeTypes, rand);

                randomizedEdgeTypes.Add(nextType);
                remainingEdgeTypes.Remove(nextType);
            }

            int maxEdgeTypes = RandUtils.IntRange(edgePattern.MinTypes, edgePattern.MaxTypes, rand);

            while (randomizedEdgeTypes.Count > maxEdgeTypes)
            {
                randomizedEdgeTypes.RemoveAt(randomizedEdgeTypes.Count - 1);
            }

            List<List<RoomEdgeGenArgs>> symmetrySets = new List<List<RoomEdgeGenArgs>>();

            List<RoomEdgeGenArgs> finalEdges = new List<RoomEdgeGenArgs>();

            if (edgesAreSymmetric && edgePattern.Quantity > 1)
            {
                List<RoomEdgeGenArgs> firstList = new List<RoomEdgeGenArgs>();
                List<RoomEdgeGenArgs> secondList = new List<RoomEdgeGenArgs>();
                if (rand.NextDouble() < 0.5f)
                {
                    firstList = new List<RoomEdgeGenArgs>(upDownList);
                    secondList = new List<RoomEdgeGenArgs>(leftRightList);
                }
                else
                {
                    firstList = new List<RoomEdgeGenArgs>(leftRightList);
                    secondList = new List<RoomEdgeGenArgs>(upDownList);
                }
                RoomEdgeType firstType = RandUtils.GetRandomElement(randomizedEdgeTypes, rand);
                foreach (RoomEdgeGenArgs edgeArgs in firstList)
                {
                    edgeArgs.EdgeType = firstType;
                    allEdgeArgsList.Remove(edgeArgs);
                }
                finalEdges.AddRange(firstList);
                symmetrySets.Add(firstList);

                if (edgePattern.Quantity == 4)
                {
                    RoomEdgeType secondType = RandUtils.GetRandomElement(randomizedEdgeTypes, rand);
                    randomizedEdgeTypes.RemoveAt(0);
                    foreach (RoomEdgeGenArgs edgeArgs in secondList)
                    {
                        edgeArgs.EdgeType = secondType;
                        allEdgeArgsList.Remove(edgeArgs);
                    }
                    finalEdges.AddRange(secondList);
                    symmetrySets.Add(secondList);
                }
            }

            while (finalEdges.Count < edgePattern.Quantity && allEdgeArgsList.Count > 0 && randomizedEdgeTypes.Count > 0)
            {
                RoomEdgeType edgeType = RandUtils.GetRandomElement(randomizedEdgeTypes, rand);

#if UNITY_EDITOR
                if (BaseEdgeGenHelper.ForcedRoomEdgeType > 0)
                {
                    edgeType = roomTypeSettings.Get(BaseEdgeGenHelper.ForcedRoomEdgeType);
                }
#endif

                RoomEdgeGenArgs currArgs = allEdgeArgsList[rand.Next() % allEdgeArgsList.Count];
                allEdgeArgsList.Remove(currArgs);
                finalEdges.Add(currArgs);
                currArgs.EdgeType = edgeType;
                symmetrySets.Add(new List<RoomEdgeGenArgs>() { currArgs });
            }


            foreach (List<RoomEdgeGenArgs> genArgsList in symmetrySets)
            {

                RoomEdgeType etype = genArgsList[0].EdgeType;

                float depthRatio = RandUtils.FloatRange(etype.MinDepthRatio, etype.MaxDepthRatio, levelArgs.Rand);
                float endDoorChance = RandUtils.FloatRange(etype.MinEndDoorChance, etype.MaxEndDoorChance, levelArgs.Rand);
                float missingChance = RandUtils.FloatRange(etype.MinMissingChance, etype.MaxMissingChance, levelArgs.Rand);

                bool offset = levelArgs.Rand.NextDouble() < etype.OffsetChance;

                bool leftOffset = false;
                bool rightOffset = false;
                if (offset)
                {
                    if (levelArgs.Rand.NextDouble() < 0.5f)
                    {
                        leftOffset = true;
                    }
                    else
                    {
                        rightOffset = true;
                    }
                }
                bool narrow = levelArgs.Rand.NextDouble() < etype.NarrowChance;

                int seed = levelArgs.Rand.Next();

                foreach (RoomEdgeGenArgs edgeArgs in genArgsList)
                {
                    edgeArgs.SetSecondaryData(seed, depthRatio, narrow, leftOffset, rightOffset, endDoorChance, missingChance);
                }
            }

            foreach (RoomEdgeGenArgs edgeArgs in finalEdges)
            {

                if (_edgeGenHelpers.TryGetValue(edgeArgs.EdgeType.IdKey, out IEdgeGenHelper helper))
                {
                    await helper.GenerateEdge(edgeArgs, genData, levelArgs);
                }
            }

            await Task.CompletedTask;
        }
    }
}
