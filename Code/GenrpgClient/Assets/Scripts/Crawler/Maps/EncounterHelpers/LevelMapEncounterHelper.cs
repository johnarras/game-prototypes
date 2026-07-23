using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Loading;
using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Maps.EncounterHelpers
{
    public class LevelMapEncounterHelper : BaseClientMapEncounterHelper
    {
        public override long HelperKey => MapEncounters.LevelMap;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int x, int z, CancellationToken token)
        {

            CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
            {
                MapRoot = mapRoot,
                Cell = cell,
            };

            _mapService.LoadProp(loadData, "LevelMap", token);


            await Task.CompletedTask;
        }

        public override async ValueTask OnEnterCell(PartyData party, CrawlerMap map, CrawlerMapStatus mapStatus, CrawlerMoveStatus moveStatus, CancellationToken token)
        {
            if (!party.CompletedMaps.HasBitIndex(party.CurrPos.MapId))
            {
                _crawlerService.ChangeState(ECrawlerStates.LevelMap, token);
                moveStatus.MoveIsStopped = true;
            }
            await Task.CompletedTask;
        }
    }
}


