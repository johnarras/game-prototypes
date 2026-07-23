using OxDb.Client.Crawler.MapGen.Services;
using OxDb.Client.Crawler.Maps.Services;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.Client.GameObjects;
using OxDb.Client.UI.Interfaces;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Buildings.Constants;
using OxDb.SharedGame.Crawler.MapGen.Entities;
using OxDb.SharedGame.Crawler.MapGen.Helpers;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Options.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Names.Services;
using OxDb.SharedGame.ProcGen.Services;
using OxDb.SharedGame.Units.Settings;
using OxDb.SharedGame.Zones.Constants;
using OxDb.SharedGame.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.MapGen.Helpers
{

    public class NpcQuestMaps
    {
        public List<MapLink> PrimaryMaps { get; set; } = new List<MapLink>();
        public List<MapLink> SecondaryMaps { get; set; } = new List<MapLink>();
    }

    public class MapLink
    {
        public CrawlerMap Map { get; set; }
        public MapCellDetail Link { get; set; }
    }

    public abstract class BaseCrawlerMapGenHelper : ICrawlerMapGenHelper
    {

        protected IAssetService _assetService = null;
        protected IUIService _uiService = null;
        protected ILogService _logService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IClientEntityService _clientEntityService = null;
        protected ICrawlerWorldService _worldService = null;
        protected ICrawlerMapService _crawlerMapService = null;
        protected ICrawlerMapGenService _mapGenService = null;
        protected IZoneGenService _zoneGenService = null;
        protected INameGenService _nameGenService = null;
        protected ILineGenService _lineGenService = null;
        protected ICrawlerOptionsService _optionsService = null;
        protected ISamplingService _samplingService = null;

        public abstract long HelperKey { get; }

        public abstract Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData crawlerMapGenData, CancellationToken token);
        public abstract NpcQuestMaps GetQuestMapsForNpc(PartyData party, CrawlerWorld world, CrawlerMap map, MapCellDetail npcDetail, IRandom rand);

        /// <summary>
        /// Add a bunch of random lines within a given 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="genData"></param>
        /// <param name="rand"></param>
        /// <param name="density"></param>
        /// <returns></returns>
        protected bool[,] AddCorridors(CrawlerMap map, CrawlerMapGenData genData, IRandom rand, float density = 1.0f)
        {
            bool[,] clearCells = new bool[map.Width, map.Height];
            clearCells[map.Width / 2, map.Height / 2] = true;

            List<Point2I> endPoints = new List<Point2I> { new Point2I(map.Width / 2, map.Height / 2) };

            int corridorCount = (int)(Math.Sqrt((map.Width * map.Height)) * density * 0.5f);

            int edgeSize = (map.CrawlerMapTypeId == CrawlerMapTypes.City ? 2 : 1);
            int maxLength = Math.Max(5, (map.Width + map.Height) / 2);

            if (maxLength > 10)
            {
                maxLength = 10;
            }
            for (int times = 0; times < corridorCount; times++)
            {
                if (endPoints.Count < 1)
                {
                    break;
                }

                int pointIndex = rand.Next() % endPoints.Count;
                if (times == 0)
                {
                    pointIndex = 0;
                }
                else
                {
                    pointIndex = Math.Min(pointIndex, rand.Next() % endPoints.Count);
                }

                Point2I startPoint = endPoints[pointIndex];

                int sx = startPoint.X;
                int sz = startPoint.Z;

                int dx = RandUtils.IntRange(maxLength / 2, maxLength, rand) * (rand.NextDouble() < 0.5f ? -1 : 1);
                int dz = RandUtils.IntRange(maxLength / 2, maxLength, rand) * (rand.NextDouble() < 0.5f ? -1 : 1);

                int ex = MathUtil.Clamp(edgeSize, sx + dx, map.Width - edgeSize - 1);
                int ez = MathUtil.Clamp(edgeSize, sz + dz, map.Height - edgeSize - 1);


                bool xFirst = true;

                if (rand.NextDouble() > 0.5f)
                {
                    xFirst = false;
                }


                List<Point2I> newPoints = _lineGenService.GridConnect(sx, sz, ex, ez, xFirst);


                foreach (Point2I pt in newPoints)
                {
                    clearCells[pt.X, pt.Z] = true;
                }


                int pointsToAdd = (int)Math.Max(1, Math.Ceiling(newPoints.Count * 0.3f));

                for (int i = 0; i < pointsToAdd; i++)
                {
                    if (newPoints.Count > 0)
                    {
                        Point2I newPoint = newPoints[rand.Next() % newPoints.Count];
                        endPoints.Add(newPoint);
                        newPoints.Remove(newPoint);
                    }
                }

                endPoints.Add(new Point2I(ex, ez));
            }

            return clearCells;
        }

        protected async Task AddMapNpcs<TPoint>(PartyData party, CrawlerWorld world, CrawlerMapGenData genData, CrawlerMap map, List<TPoint> okPoints, IRandom rand) where TPoint : Point2I
        {
            await Task.CompletedTask;
            if (!_optionsService.HasOption(party, CrawlerOptions.Quests))
            {
                return;
            }
            if (_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                if (rand.NextDouble() > genData.MapType.NpcChance)
                {
                    return;
                }
            }

            List<TPoint> nonWaterOpenPoints = new List<TPoint>();

            foreach (TPoint tpt in okPoints)
            {
                if (map.Get(tpt.X, tpt.Z, CellIndex.Terrain) != ZoneTypes.Water)
                {
                    nonWaterOpenPoints.Add(tpt);
                }
            }

            okPoints = nonWaterOpenPoints;

            List<MapCellDetail> entrances = map.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

            List<MapCellDetail> blockedEntrances = new List<MapCellDetail>();
            foreach (MapCellDetail detail in entrances)
            {
                CrawlerMap map2 = world.GetMap(detail.EntityId);

                if (map2 != null && (map2.CrawlerMapTypeId == CrawlerMapTypes.Outdoors ||
                    map2.CrawlerMapTypeId == CrawlerMapTypes.City))
                {
                    blockedEntrances.Add(detail);
                }
            }

            okPoints = okPoints.Where(x => !blockedEntrances.FastAny(e =>
            MathUtil.PythagoreanDistance(x.X - e.X, x.Z - e.Z)
            <= genData.MapType.MinDistanceToEntrance)).ToList();

            int minDistanceBetweenNpcs = Math.Max(3, genData.MapType.MinNpcSeparation);

            int npcQuantity = RandUtils.IntRange(genData.MapType.MinNpcQuantity, genData.MapType.MaxNpcQuantity, rand);

            if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld) && npcQuantity > 1)
            {
                npcQuantity = 1;
            }

            ZoneType cityZoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(ZoneTypes.City);

            TribeType humanoidTribe = _gameData.Get<TribeSettings>(_gs.ch).GetData().FirstOrDefault(x => x.Name.IndexOf("Human") == 0);

            UnitTypeSettings unitSettings = _gameData.Get<UnitTypeSettings>(_gs.ch);

            List<UnitType> okUnitTypes = new List<UnitType>();

            foreach (ZoneUnitSpawn spawn in cityZoneType.ZoneUnitSpawns)
            {
                UnitType unitType = unitSettings.Get(spawn.UnitTypeId);
                if (unitType != null && unitType.TribeTypeId == humanoidTribe.IdKey)
                {
                    okUnitTypes.Add(unitType);
                }
            }

            if (okUnitTypes.Count < 1)
            {
                okUnitTypes.AddRange(unitSettings.GetData());
            }

            for (int i = 0; i < npcQuantity && okPoints.Count > 0; i++)
            {
                TPoint chosenPoint = okPoints[rand.Next() % okPoints.Count];

                okPoints.Remove(chosenPoint);

                okPoints = okPoints.Where(pt => MathUtil.PythagoreanDistance(pt.X - chosenPoint.X, pt.Z - chosenPoint.Z)
                >= minDistanceBetweenNpcs).ToList();

                UnitType unitType = okUnitTypes[rand.Next() % okUnitTypes.Count];

                long nextIdkey = CollectionUtils.GetNextIdKey(world.Npcs);

                CrawlerNpc npc = new CrawlerNpc()
                {
                    UnitTypeId = unitType.IdKey,
                    IdKey = nextIdkey,
                    Name = _nameGenService.GenerateUnitName(rand, true),
                    Level = _worldService.GetMapLevelAtPoint(world, map.IdKey, chosenPoint.X, chosenPoint.Z),
                    MapId = map.IdKey,
                    X = chosenPoint.X,
                    Z = chosenPoint.Z,
                };

                world.Npcs.Add(npc);

                map.Details.Add(new MapCellDetail() { EntityTypeId = EntityTypes.Npc, EntityId = npc.IdKey, X = chosenPoint.X, Z = chosenPoint.Z });

                map.SetEntity(chosenPoint.X, chosenPoint.Z, EntityTypes.Building, BuildingTypes.Npc);
            }
        }
    }
}

