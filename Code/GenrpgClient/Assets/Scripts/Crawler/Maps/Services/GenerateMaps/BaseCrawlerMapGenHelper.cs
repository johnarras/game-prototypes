using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.Maps.Services;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Crawler.MapGen.Services;
using Genrpg.Shared.Crawler.MapGen.Entities;
using Genrpg.Shared.Crawler.MapGen.Helpers;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Assets;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Worlds.Entities;
using System.Linq;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Names.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Zones.Constants;
using Genrpg.Shared.Units.Settings;
using System.Net;
using System.Text;
using Genrpg.Shared.Crawler.Maps.Settings;
using System.Threading;

namespace Assets.Scripts.Crawler.Maps.Services.GenerateMaps
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

        public abstract long Key { get; }

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

            List<MyPoint> endPoints = new List<MyPoint> { new MyPoint(map.Width / 2, map.Height / 2) };

            int corridorCount = (int)(Math.Sqrt((map.Width * map.Height)) * density * 0.3f);

            int edgeSize = (map.CrawlerMapTypeId == CrawlerMapTypes.City ? 2 : 1);
            int maxLength = Math.Max(5, (map.Width + map.Height) / 4);

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

                MyPoint startPoint = endPoints[rand.Next() % endPoints.Count];

                int sx = startPoint.X;
                int sz = startPoint.Y;  

                int dx = MathUtils.IntRange(maxLength/2, maxLength, rand)*(rand.NextDouble() < 0.5f ? -1 : 1);
                int dz = MathUtils.IntRange(maxLength/2, maxLength, rand) * (rand.NextDouble() < 0.5f ? -1 : 1);

                if (sx + dx < edgeSize || sx+dx >= map.Width-edgeSize)
                {
                    dx = -dx;
                }
                if (sz + dz < edgeSize || sz+dz >= map.Height-edgeSize)
                {
                    dz = -dz;
                }
                int ex = MathUtils.Clamp(edgeSize, sx + dx, map.Width - edgeSize);
                int ez = MathUtils.Clamp(edgeSize, sz + dz, map.Height - edgeSize);

                int mx = 0;
                int mz = 0;

                if (rand.NextDouble() < 0.5)
                {
                    mx = ex;
                    mz = sz;
                    clearCells[mx, mz] = true;
                    clearCells[ex, ez] = true;
                    for (int x = sx; x != ex; x += Math.Sign(ex - sx))
                    {
                        clearCells[x, sz] = true;
                    }
                    for (int z = sz; z != ez; z += Math.Sign(ez - sz))
                    {
                        clearCells[ex, z] = true;
                    }
                }
                else
                {
                    mx = sx;
                    mz = ez;
                    clearCells[mx, mz] = true;
                    clearCells[ex, ez] = true;
                    for (int x = sx; x != ex; x += Math.Sign(ex - sx))
                    {
                        clearCells[x, ez] = true;
                    }
                    for (int z = sz; z != ez; z += Math.Sign(ez - sz))
                    {
                        clearCells[sx, z] = true;
                    }
                }
               

                endPoints.Add(new MyPoint(mx, mz));
                endPoints.Add(new MyPoint(ex, ez));
            }

            return clearCells;
        }

        protected async Task AddMapNpcs(PartyData party, CrawlerWorld world, CrawlerMapGenData genData, CrawlerMap map, List<PointXZ> okPoints, IRandom rand)
        {
            if (rand.NextDouble() > genData.MapType.NpcChance)
            {
                return;
            }

            List<MapCellDetail> entrances = map.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

            okPoints = okPoints.Where(x => !entrances.Any(e => 
            MathUtils.PythagoreanDistance( x.X - e.X, x.Z - e.Z) 
            <= genData.MapType.MinDistanceToEntrance)).ToList();

            int minDistanceBetweenNpcs = Math.Max(3, genData.MapType.MinNpcSeparation);

            int npcQuantity = MathUtils.IntRange(genData.MapType.MinNpcQuantity, genData.MapType.MaxNpcQuantity, rand);

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
                PointXZ chosenPoint = okPoints[rand.Next()%okPoints.Count];

                okPoints.Remove(chosenPoint);

                okPoints = okPoints.Where(pt => MathUtils.PythagoreanDistance( pt.X-chosenPoint.X, pt.Z-chosenPoint.Z) 
                >= minDistanceBetweenNpcs).ToList();

                UnitType unitType = okUnitTypes[rand.Next()%okUnitTypes.Count];

                long nextIdkey = CollectionUtils.GetNextIdKey(world.Npcs);

                CrawlerNpc npc = new CrawlerNpc()
                {
                    UnitTypeId = unitType.IdKey,
                    IdKey = nextIdkey,
                    Name = _nameGenService.GenerateUnitName(rand, true),
                    Level = await _worldService.GetMapLevelAtPoint(world, map.IdKey, chosenPoint.X, chosenPoint.Z),
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