using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.DrawEntityHelpers;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.ProcGen.Settings.Props;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers
{
    public class PropDrawCellHelper : BaseCrawlerDrawCellHelper
    {
        public override int Order => 450;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int xpos, int zpos, int realCellX, int realCellZ, CancellationToken token)
        {
            long propTypeId = mapRoot.Map.GetEntityId(cell.MapX, cell.MapZ, EntityTypes.Prop);
            if (propTypeId > 0)
            {
                PropType propType = _gameData.Get<PropTypeSettings>(null).Get(propTypeId);

                if (propType != null)
                {
                    int variation = 1;
                    if (propType.NumChoices > 1)
                    {
                        variation = 1 + (cell.MapX * 31 + cell.MapZ * 47) % propType.NumChoices;
                    }

                    CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                    {
                        Angle = mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Dir) * CrawlerMapConstants.DirToAngleMult,
                        Cell = cell,
                        MapRoot = mapRoot,
                        Seed = _mapService.GetMapCellHash(mapRoot.Map.IdKey, cell.MapX, cell.MapZ, propTypeId * 17),
                        AssetCategoryNameOverride = AssetCategoryNames.Props,
                    };

                    _mapService.LoadProp(loadData, propType.Art + variation, token);

                }
            }

            await Task.CompletedTask;
        }
    }
}


