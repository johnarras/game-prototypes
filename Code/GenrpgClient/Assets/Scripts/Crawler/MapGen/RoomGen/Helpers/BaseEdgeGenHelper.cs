using OxDb.Client.Crawler.MapGen.Helpers;
using OxDb.Client.Crawler.MapGen.RoomGen.Entities;
using OxDb.Client.Crawler.MapGen.Services;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Zones.Constants;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.MapGen.RoomGen.Helpers
{
    public abstract class BaseEdgeGenHelper : IEdgeGenHelper
    {

#if UNITY_EDITOR
        private static long _forcedRoomEdgeType { get; set; }
        public static long ForcedRoomEdgeType => _forcedRoomEdgeType;
#endif

        protected ILogService _logService = null;
        protected ICrawlerMapGenService _mapGenService = null;

        public abstract long HelperKey { get; }

        protected virtual void PreInitSetup(RoomEdgeGenArgs edgeArgs, CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs)
        {

        }

        public virtual async ValueTask GenerateEdge(RoomEdgeGenArgs edgeArgs, CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs)
        {

            PreInitSetup(edgeArgs, genData, levelArgs);
            List<EdgeRowArgs> rowArgs = GetEdgeRowArgs(edgeArgs, genData, levelArgs);

            foreach (EdgeRowArgs rowArg in rowArgs)
            {
                await PlaceEdgeRowArgs(rowArg, edgeArgs, genData, levelArgs);
            }

            await Task.CompletedTask;
        }

        virtual protected async ValueTask PlaceEdgeRowArgs(EdgeRowArgs rowArgs, RoomEdgeGenArgs edgeArgs, CrawlerMapGenData genData,
            DungeonLevelGenArgs levelArgs)
        {
            await Task.CompletedTask;
            int maxLengthGone = 0;
            for (int l = 1; l <= rowArgs.Length; l++)
            {
                int cx = rowArgs.SX + rowArgs.DX * l;
                int cz = rowArgs.SZ + rowArgs.DZ * l;

                // Cannot be at absolute edge of map due to needing to do adjacent room id sets.
                if (cx < 1 || cx >= levelArgs.Map.Width - 1 ||
                    cz < 1 || cz >= levelArgs.Map.Height - 1)
                {
                    continue;
                }

                if (levelArgs.AdjacentRoomIds[cx, cz] != 0 && levelArgs.AdjacentRoomIds[cx, cz] != edgeArgs.RoomId)
                {
                    break;
                }

                levelArgs.RoomIds[cx, cz] = edgeArgs.RoomId;
                levelArgs.Map.Set(cx, cz, CellIndex.Terrain, genData.ZoneType.IdKey);
                levelArgs.SetFlag(cx, cz, DungeonLevelFlags.EdgeCell);
                for (int xx = cx - 1; xx <= cx + 1; xx++)
                {
                    for (int zz = cz - 1; zz <= cz + 1; zz++)
                    {
                        if (levelArgs.AdjacentRoomIds[xx, zz] == 0)
                        {
                            levelArgs.AdjacentRoomIds[xx, zz] = edgeArgs.RoomId;
                        }
                    }
                }

                maxLengthGone++;
            }

            if (maxLengthGone > 0)
            {
                int nx = rowArgs.SX + rowArgs.DX * (maxLengthGone);
                int nz = rowArgs.SZ + rowArgs.DZ * (maxLengthGone);
                levelArgs.SetFlag(nx, nz, DungeonLevelFlags.EdgeEndCell);

                if (rowArgs.RoomAtEnd)
                {
                    MapDir currMapDir = MapDirUtils.GetDirFromDeltas(rowArgs.DX, rowArgs.DZ);

                    _mapGenService.AddRoomWithDoor(levelArgs, nx, nz, currMapDir.OppDir, genData.ZoneType.IdKey);
                }
            }
        }

        protected virtual int GetRowLength(RoomEdgeGenArgs edgeArgs, int pindex, IRandom rand)
        {
            return 0;
        }

        protected virtual List<EdgeRowArgs> GetEdgeRowArgs(RoomEdgeGenArgs edgeArgs, CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs)
        {

            IRandom rand = new MyRandom(edgeArgs.Seed);
            List<EdgeRowArgs> retval = new List<EdgeRowArgs>();

            List<Point2I> edgePoints = edgeArgs.GetEdgePoints();

            if (edgePoints.Count < 1)
            {
                return retval;
            }

            for (int p = 0; p < edgePoints.Count; p++)
            {
                if (rand.NextDouble() < edgeArgs.MissingChance)
                {
                    continue;
                }

                EdgeRowArgs args = new EdgeRowArgs()
                {
                    DX = edgeArgs.DX,
                    DZ = edgeArgs.DZ,
                    Length = GetRowLength(edgeArgs, p, rand),
                    SX = edgePoints[p].X,
                    SZ = edgePoints[p].Z,
                    RoomAtEnd = rand.NextDouble() < edgeArgs.EndDoorChance,
                };

                if (args.Length > 0)
                {
                    retval.Add(args);
                }
            }

            return retval;
        }
    }
}
