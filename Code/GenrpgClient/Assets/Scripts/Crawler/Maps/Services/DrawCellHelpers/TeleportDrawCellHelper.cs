using OxDb.Client.Crawler.Maps.Constants;
using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Loading;
using OxDb.Client.Crawler.Maps.Services.DrawEntityHelpers;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Maps.Services.DrawCellHelpers
{
    public class TeleportDrawCellHelper : BaseCrawlerDrawCellHelper
    {
        public override ECrawlerDrawCellOrder HelperKey => ECrawlerDrawCellOrder.Teleport;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, CancellationToken token)
        {
            MapCellDetail teleportDetail = mapRoot.Map.Details.FirstOrDefault(d => d.X == cell.MapX && d.Z == cell.MapZ &&
            d.EntityTypeId == EntityTypes.TeleportIn);
            if (teleportDetail != null)
            {
                CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                {
                    Angle = mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Dir) * CrawlerMapConstants.DirToAngleMult,
                    Cell = cell,
                    MapRoot = mapRoot,
                    Seed = _mapService.GetMapCellHash(mapRoot.Map.IdKey, cell.MapX, cell.MapZ, 17),

                };

                _mapService.LoadProp(loadData, "TeleportIn", token);
            }
            await Task.CompletedTask;
        }
    }
}


