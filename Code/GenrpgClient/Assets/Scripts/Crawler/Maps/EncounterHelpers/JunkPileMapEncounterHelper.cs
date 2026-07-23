using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Loading;
using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.SharedGame.Crawler.Loot.Services;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Maps.EncounterHelpers
{
    public class JunkPileMapEncounterHelper : BaseClientMapEncounterHelper
    {
        protected ILootGenService _lootGenService = null;

        public override long HelperKey => MapEncounters.JunkPile;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int x, int z, CancellationToken token)
        {
            CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
            {
                MapRoot = mapRoot,
                Cell = cell,
            };

            _mapService.LoadProp(loadData, "JunkPile", token);

            await Task.CompletedTask;
        }

        public override async ValueTask OnEnterCell(PartyData party, CrawlerMap map, CrawlerMapStatus mapStatus, CrawlerMoveStatus moveStatus, CancellationToken token)
        {


            int index = map.GetIndex(party.CurrPos.X, party.CurrPos.Z);

            if (!mapStatus.Encounters.HasBitIndex(index))
            {
                _crawlerService.ChangeState(ECrawlerStates.SearchJunkPile, token);
                moveStatus.MoveIsStopped = true;
            }
            await Task.CompletedTask;
        }
    }
}


