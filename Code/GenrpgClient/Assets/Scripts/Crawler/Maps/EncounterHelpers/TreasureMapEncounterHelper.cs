using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Loading;
using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.SharedCore.Utils;
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
    public class TreasureMapEncounterHelper : BaseClientMapEncounterHelper
    {
        protected ILootGenService _lootGenService = null;

        public override long HelperKey => MapEncounters.Treasure;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int x, int z, CancellationToken token)
        {
            CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
            {
                MapRoot = mapRoot,
                Cell = cell,
            };

            _mapService.LoadProp(loadData, "Chest", token);

            await Task.CompletedTask;
        }

        public override async ValueTask OnEnterCell(PartyData party, CrawlerMap map, CrawlerMapStatus mapStatus, CrawlerMoveStatus moveStatus, CancellationToken token)
        {
            LootGenData lootGenData = await _lootGenService.CreateLootGenData(party,
                RandUtils.FloatRange(2.0f, 4.0f, _gs.Rand), RandUtils.FloatRange(2.0f, 4.0f, _gs.Rand), RandUtils.FloatRange(2.0f, 4.0f, _gs.Rand), "You Found a Great Treasure!", ECrawlerStates.ExploreWorld, null);

            int index = map.GetIndex(party.CurrPos.X, party.CurrPos.Z);
            mapStatus.Encounters.SetBitIndex(index);
            _mapService.ClearCellProps(party.CurrPos.X, party.CurrPos.Z);
            _crawlerService.ChangeState(ECrawlerStates.GiveLoot, token, lootGenData);
            moveStatus.MoveIsStopped = true;
        }
    }
}


