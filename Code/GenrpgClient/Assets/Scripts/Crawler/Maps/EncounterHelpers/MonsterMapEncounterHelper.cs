using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Maps.EncounterHelpers
{
    public class MonsterMapEncounterHelper : BaseClientMapEncounterHelper
    {
        public override long HelperKey => MapEncounters.Monsters;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int x, int z, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public override async ValueTask OnEnterCell(PartyData party, CrawlerMap map, CrawlerMapStatus mapStatus, CrawlerMoveStatus moveStatus, CancellationToken token)
        {
            InitialCombatState initialCombatState = new InitialCombatState()
            {
                Difficulty = 1.5f,
            };
            _crawlerService.ChangeState(ECrawlerStates.StartCombat, token, initialCombatState);
            moveStatus.MoveIsStopped = true;
            await Task.CompletedTask;
        }
    }
}


