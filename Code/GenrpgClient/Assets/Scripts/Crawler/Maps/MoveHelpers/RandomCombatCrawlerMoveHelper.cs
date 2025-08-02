using Assets.Scripts.Crawler.Maps.Services.Entities;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
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
            if (status.MoveIsComplete || !status.MovedPosition)
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

            if (_rand.NextDouble() > randomChance)
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
