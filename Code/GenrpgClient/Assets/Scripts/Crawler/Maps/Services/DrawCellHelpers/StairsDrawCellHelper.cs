using OxDb.Client.Crawler.Maps.Constants;
using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Loading;
using OxDb.Client.Crawler.Maps.Services.DrawEntityHelpers;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Maps.Services.DrawCellHelpers
{
    public class StairsDrawCellHelper : BaseCrawlerDrawCellHelper
    {
        public override ECrawlerDrawCellOrder HelperKey => ECrawlerDrawCellOrder.Stairs;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, CancellationToken token)
        {
            List<MapCellDetail> cellDetails = mapRoot.Map.Details.Where(d => d.X == cell.MapX && d.Z == cell.MapZ).ToList();
            if (mapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon)
            {

                List<MapCellDetail> exitDetails = cellDetails.Where(d => d.X == cell.MapX && d.Z == cell.MapZ && d.EntityTypeId == EntityTypes.Map).ToList();

                if (exitDetails.Count > 0)
                {
                    MapCellDetail detail = exitDetails.First();
                    bool showDownStairs = 
                        (detail.EntityId == mapRoot.Map.IdKey + 1);

                    CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                    {
                        Angle = mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Dir) * CrawlerMapConstants.DirToAngleMult,
                        Cell = cell,
                        MapRoot = mapRoot,
                        Seed = _mapService.GetMapCellHash(mapRoot.Map.IdKey, cell.MapX, cell.MapZ, 1),
                        Scale = (mapRoot.Map.IsOutdoorDungeon() ? 0.5f : 1),
                    };


                    _mapService.LoadProp(loadData, (showDownStairs ? "StairsDown" : "StairsUp"), token);
                }
            }


            await Task.CompletedTask;
        }
    }
}


