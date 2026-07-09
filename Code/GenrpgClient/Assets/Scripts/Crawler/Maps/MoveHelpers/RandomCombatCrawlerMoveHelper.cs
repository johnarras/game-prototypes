using Assets.Scripts.Crawler.Maps.Services.Entities;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using OxDb.SharedGame.Crawler.Combat.Settings;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public class RandomCombatCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 500;

        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            if (status.MoveIsComplete || !status.MovedPosition || !_optionService.HasOption(party, CrawlerOptions.RandomMonsters))
            {
                return;
            }

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);

            LastMoveStatus lastMove = _moveService.GetLastMoveStatus();

            if (lastMove.MovesSinceLastCombat < combatSettings.MovesBetweenEncounters)
            {
                return;
            }

            double randomChance = combatSettings.RandomEncounterChance;

            if (_gs.Rand.NextDouble() > randomChance)
            {
                return;
            }

            _moveService.ClearMovement();
            _crawlerService.ChangeState(ECrawlerStates.StartCombat, token);
            status.MoveIsComplete = true;
            await Task.CompletedTask;
        }
    }
}


