using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.Client.FloatingText.ClientEvents;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Riddles.Services;
using OxDb.SharedGame.Riddles.Settings;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.MoveHelpers
{
    public class RiddleMapMoveHelper : BaseCrawlerMoveHelper
    {
        private IRiddleService _riddleService = null;

        public override ECrawlerMoveOrder HelperKey => ECrawlerMoveOrder.Riddles;


        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            if (!status.MovedPosition || status.MoveIsStopped)
            {
                return;
            }

            if (party.RiddlesCompleted.HasBitIndex(status.MapRoot.Map.IdKey))
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
                status.MoveIsStopped = true;
            }
            await Task.CompletedTask;
        }
    }
}


