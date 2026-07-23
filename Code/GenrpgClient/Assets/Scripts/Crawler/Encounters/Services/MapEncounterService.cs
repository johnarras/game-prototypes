using OxDb.Client.Crawler.Maps.Services;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Loot.Services;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Services;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Encounters.Services
{
    public interface IMapEncounterService : IInjectable
    {
        ValueTask SearchJunkPile(CancellationToken token);
        public void ClearEncounterAtCell(long mapId, int x, int z);
        public void ClearEncounterAtParty();
    }

    public class MapEncounterService : IMapEncounterService
    {

        private ICrawlerService _crawlerService = null;
        private ICrawlerMapService _mapService = null;
        private ICrawlerWorldService _worldService = null;
        private ILootGenService _lootGenService = null;
        private IClientGameState _gs = null;

        public void ClearEncounterAtParty()
        {
            PartyData party = _crawlerService.GetParty();
            ClearEncounterAtCell(party.CurrPos.MapId, party.CurrPos.X, party.CurrPos.Z);
        }

        public void ClearEncounterAtCell(long mapId, int x, int z)
        {
            PartyData party = _crawlerService.GetParty();
            CrawlerMap map = _worldService.GetMap(mapId);
            CrawlerMapStatus mapStatus = party.GetMapStatus(mapId, true);

            int index = map.GetIndex(x, z);
            mapStatus.Encounters.SetBitIndex(index);
            _mapService.ClearCellProps(x, z);

            return;
        }

        public async ValueTask SearchJunkPile(CancellationToken token)
        {
            PartyData party = _crawlerService.GetParty();

            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);
            CrawlerMapStatus mapStatus = party.GetMapStatus(party.CurrPos.MapId, true);

            ClearEncounterAtParty();

            if (_gs.Rand.NextDouble() < 0.5f)
            {
                LootGenData lootGenData = await _lootGenService.CreateLootGenData(party,
                    RandUtils.FloatRange(2.0f, 4.0f, _gs.Rand), RandUtils.FloatRange(2.0f, 4.0f, _gs.Rand), RandUtils.FloatRange(2.0f, 4.0f, _gs.Rand), "You Found a Great Treasure!", ECrawlerStates.ExploreWorld, null);

                _crawlerService.ChangeState(ECrawlerStates.GiveLoot, token, lootGenData);
            }
            else
            {
                long seed = party.WorldId + party.Seed + map.ArtSeed + party.CurrPos.X + party.CurrPos.Z;

                MyRandom rand = new MyRandom(seed);


                float difficulty = 1.5f;

                while (rand.NextDouble() < 0.4f)
                {
                    difficulty += 0.25f;
                }

                InitialCombatState initialCombatState = new InitialCombatState()
                {
                    Difficulty = difficulty,
                };
                _crawlerService.ChangeState(ECrawlerStates.StartCombat, token, initialCombatState);
            }
        }
    }
}
