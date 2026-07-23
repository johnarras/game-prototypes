using OxDb.Client.Assets.Scripts.Crawler.Maps.Services;
using OxDb.Client.Crawler.Maps.Constants;
using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Loading;
using OxDb.Client.Crawler.Maps.Services.DrawEntityHelpers;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Zones.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Maps.Services.DrawCellHelpers
{
    public class PropDrawCellHelper : BaseCrawlerDrawCellHelper
    {

        private ICrawlerPropService _propService = null;

        public override ECrawlerDrawCellOrder HelperKey => ECrawlerDrawCellOrder.Props;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, CancellationToken token)
        {

            if (cell.WorldX != cell.MapX || cell.WorldZ != cell.MapZ)
            {
                return;
            }

            if (mapRoot.Map.GetEntityId(cell.MapX, cell.MapZ, EntityTypes.RoomEdge) > 0)
            {
                return;
            }
            bool blockedOutdoorDungeonCell = false;

            bool isOutdoorDungeon = false;

            long zoneTypeId = mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Terrain);

            ZoneRegion region = mapRoot.Map.GetRegion(cell.MapX, cell.MapZ);

            if (region != null)
            {
                zoneTypeId = region.ZoneTypeId; 
            }

            ZoneType zoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zoneTypeId);

            long propTypeId = mapRoot.Map.GetEntityId(cell.MapX, cell.MapZ, EntityTypes.Prop);

            bool hasLargeProp = propTypeId > 0;

            int dir = mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Dir);

            if (mapRoot.Map.IsOutdoorDungeon())
            {
                isOutdoorDungeon = true;
                if (zoneType == null)
                {
                    blockedOutdoorDungeonCell = true;
                    zoneTypeId = mapRoot.Map.ZoneTypeId;

                    zoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zoneTypeId);

                    if (zoneType == null)
                    {
                        return;
                    }

                    hasLargeProp = true;
                }
                else
                {
                    hasLargeProp = false;
                }
            }
            else
            {
                if (zoneType == null)
                {
                    return;
                }
            }

            DrawCellPropsArgs args = new DrawCellPropsArgs()
            {
                Party = party,
                MapRoot = mapRoot,
                BlockedOutdoorDungeonCell = blockedOutdoorDungeonCell,
                Cell = cell,
                Dir = dir,
                IsOutdoorDungeon = isOutdoorDungeon,
                HasLargeProp = propTypeId > 0,
                World = world,
                ZoneType = zoneType,
            };

            await _propService.DrawPropsAtCell(args, token);

            await Task.CompletedTask;
        }
    }
}


