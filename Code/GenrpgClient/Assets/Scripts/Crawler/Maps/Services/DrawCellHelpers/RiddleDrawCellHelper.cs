using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.DrawEntityHelpers;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Riddles.Services;
using OxDb.SharedGame.Riddles.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers
{
    public class RiddleDrawCellHelper : BaseCrawlerDrawCellHelper
    {

        private IRiddleService _riddleService = null;
        public override int Order => 600;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int xpos, int zpos, int realCellX, int realCellZ, CancellationToken token)
        {
            int riddleIndex = mapRoot.Map.GetEntityId(cell.MapX, cell.MapZ, EntityTypes.Riddle);

            if (riddleIndex > 0 && !party.RiddlesCompleted.HasBitIndex(mapRoot.Map.IdKey))
            {
                RiddleType riddleType = _gameData.Get<RiddleTypeSettings>(_gs.ch).Get(mapRoot.Map.RiddleHints?.RiddleTypeId ?? 0);

                if (riddleType != null && _riddleService.ShouldDrawProp(party, realCellX, realCellZ))
                {
                    CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                    {
                        Angle = mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Dir) * CrawlerMapConstants.DirToAngleMult,
                        Cell = cell,
                        MapRoot = mapRoot,
                        Seed = _mapService.GetMapCellHash(mapRoot.Map.IdKey, cell.MapX, cell.MapZ, riddleIndex * 13),

                    };

                    _mapService.LoadProp(loadData, riddleType.Art, token);
                }
            }

            await Task.CompletedTask;
        }
    }
}


