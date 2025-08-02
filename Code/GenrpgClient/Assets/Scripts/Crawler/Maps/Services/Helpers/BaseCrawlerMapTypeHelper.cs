
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Genrpg.Shared.Crawler.Maps.Services;
using Assets.Scripts.Assets;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Crawler.MapGen.Services;
using Assets.Scripts.Crawler.Quests.ClientEvents;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public abstract class BaseCrawlerMapTypeHelper : ICrawlerMapTypeHelper
    {
        protected IAssetService _assetService = null;
        protected IUIService _uIInitializable = null;
        protected ILogService _logService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IClientEntityService _clientEntityService = null;
        protected ICrawlerWorldService _worldService = null;
        protected ICrawlerMapService _mapService = null;
        protected ICrawlerMapGenService _mapGenService = null; 
        protected IDispatcher _dispatcher = null;
        protected IClientRandom _rand = null;

        public abstract long Key { get; }

        public virtual async Awaitable<CrawlerMapRoot> EnterMap(PartyData party, EnterCrawlerMapData mapData, CancellationToken token)
        {
            if (party.CurrPos.MapId != mapData.MapId)
            {
                party.CurrentMap.Clear();
            }

            if (party.CurrPos.X < 0 || party.CurrPos.Z < 0)
            {
                mapData.MapX = mapData.Map.Width / 2;
                mapData.MapZ = mapData.Map.Height / 2;
            }

            long oldMapId = party.CurrPos.MapId;
            party.CurrPos.MapId = mapData.MapId;
            party.CurrPos.X = mapData.MapX;
            party.CurrPos.Z = mapData.MapZ;
            party.CurrPos.Rot = mapData.MapRot;
            CrawlerMap map = mapData.Map;

            if (!party.CompletedMaps.HasBit(mapData.Map.IdKey))
            {
                CrawlerMapStatus status = party.GetMapStatus(mapData.Map.IdKey, true);
            }

            GameObject go = new GameObject() { name = Key.ToString() };
            CrawlerMapRoot mapRoot = _clientEntityService.GetOrAddComponent<CrawlerMapRoot>(go);

            mapRoot.SetupFromMap(map);
            mapRoot.DrawX = party.CurrPos.X * CrawlerMapConstants.XZBlockSize;
            mapRoot.DrawZ = party.CurrPos.Z * CrawlerMapConstants.XZBlockSize;
            mapRoot.DrawY = CrawlerMapConstants.YBlockSize / 2;
            mapRoot.DrawRot = party.CurrPos.Rot;

            _dispatcher.Dispatch(new UpdateQuestUI());
            await Task.CompletedTask;
            return mapRoot;
        }

      
        /// <summary>
        /// Find blocking bits for a given coordinate.
        /// </summary>
        /// <param name="mapRoot"></param>
        /// <param name="sx">Can be out of range 0-map.Width-1</param>
        /// <param name="sz">Can be out of range 0-map.Height-1</param>
        /// <param name="ex">Can be out of range 0-map.Width-1</param>
        /// <param name="ez">Can be out of range 0-map.Height-1</param>
        /// <returns></returns>
        public virtual int GetBlockingBits(CrawlerMap map, int sx, int sz, int ex, int ez, bool allowBuildingEntry)
        {
            int blockBits = 0;
            if (ex > sx) // East
            {
                blockBits = map.EastWall(sx, sz);
            }
            else if (ex < sx) // West
            {
                blockBits = map.EastWall((sx + map.Width - 1) % map.Width, sz);
            }
            else if (ez > sz) // Up
            {
                blockBits = map.NorthWall(sx, sz);
            }
            else if (ez < sz) // Down
            {
                blockBits = map.NorthWall(sx, (sz + map.Height - 1) % map.Height);
            }

            int safeEx = (ex + map.Width) % map.Width;
            int safeEz = (ez + map.Height) % map.Height;

            if (map.Get(safeEx, safeEz, CellIndex.Terrain) == 0)
            {
                return WallTypes.Wall;
            }

            byte buildingId = map.GetEntityId(safeEx, safeEz, EntityTypes.Building);

            if (buildingId > 0)
            {
                if (!allowBuildingEntry)
                {
                    return WallTypes.Wall;
                }

                if (map.CrawlerMapTypeId == CrawlerMapTypes.City)
                {
                    int angle = map.Get(safeEx, safeEz, CellIndex.Dir) * CrawlerMapConstants.DirToAngleMult;

                    int dx = ex - sx;
                    int dz = ez - sz;

                    int moveAngle = ((dx == 0 ? (dz > 0 ? 90 : 270) : (dx > 0 ? 180 : 0)) + 90) % 360;

                    if (moveAngle == angle)
                    {
                        blockBits |= WallTypes.Building;
                    }
                    else
                    {
                        blockBits |= (buildingId != 0 ? WallTypes.Wall : WallTypes.None);
                    }
                }
            }
            return blockBits;
        }
    }
}
