using OxDb.Client.Assets.Constants;
using OxDb.Client.Crawler.Maps.Constants;
using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Loading;
using OxDb.Client.Crawler.Maps.Services.DrawEntityHelpers;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.ProcGen.Settings.Trees;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Maps.Services.DrawCellHelpers
{
    public class TreeDrawCellHelper : BaseCrawlerDrawCellHelper
    {
        public override ECrawlerDrawCellOrder HelperKey => ECrawlerDrawCellOrder.Trees;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, CancellationToken token)
        {
            long treeTypeId = mapRoot.Map.GetEntityId(cell.MapX, cell.MapZ, EntityTypes.Tree);
            if (treeTypeId > 0)
            {
                TreeType treeType = _gameData.Get<TreeTypeSettings>(null).Get(treeTypeId);

                if (treeType != null)
                {
                    int variation = 1;
                    if (treeType.VariationCount > 1)
                    {
                        variation = 1 + (cell.MapX * 31 + cell.MapZ * 47) % treeType.VariationCount;
                    }

                    CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                    {
                        Angle = mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Dir) * CrawlerMapConstants.DirToAngleMult,
                        Cell = cell,
                        MapRoot = mapRoot,
                        Seed = _mapService.GetMapCellHash(mapRoot.Map.IdKey, cell.MapX, cell.MapZ, treeTypeId * 17),
                        AssetCategoryNameOverride = AssetCategoryNames.Trees,
                    };

                    _mapService.LoadProp(loadData, treeType.Art + variation, token);

                }
            }

            await Task.CompletedTask;
        }
    }
}


