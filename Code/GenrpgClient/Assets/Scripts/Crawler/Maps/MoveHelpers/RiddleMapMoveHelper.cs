using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Riddles.Services;
using Genrpg.Shared.Riddles.Settings;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public class RiddleMapMoveHelper : BaseCrawlerMoveHelper
    {
        private IRiddleService _riddleService = null;

        public override int Order => 250;

        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            if (!status.MovedPosition)
            {
                return;
            }

            if (party.RiddlesCompleted.HasBit(status.MapRoot.Map.IdKey))
            {
                return;
            }

            if (status.MapRoot.Map.RiddleHints == null)
            {
                return;
            }

            int riddleIndex = status.MapRoot.Map.GetEntityId(status.EX, status.EZ, EntityTypes.Riddle);
            if (riddleIndex > 0)
            {
                RiddleType riddleType = _gameData.Get<RiddleTypeSettings>(_gs.ch).Get(status.MapRoot.Map.RiddleHints.RiddleTypeId);

                if (riddleType.IsObject && _riddleService.ShouldDrawProp(party, status.EX, status.EZ))
                {
                    _dispatcher.Dispatch(new ShowFloatingText("Odd..."));
                    return;
                }

                _crawlerService.ChangeState(ECrawlerStates.Riddle, token, status);
                status.MoveIsComplete = true;
            }
            await Task.CompletedTask;
        }
    }
}
