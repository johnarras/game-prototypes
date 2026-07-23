using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.MoveHelpers
{
    public class QuestItemMoveHelper : BaseCrawlerMoveHelper
    {
        public override ECrawlerMoveOrder HelperKey => ECrawlerMoveOrder.QuestItem;


        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            if (!status.MovedPosition)
            {
                return;
            }

            long questItemId = status.MapRoot.Map.GetEntityId(status.EX, status.EZ, EntityTypes.QuestItem);

            if (questItemId < 1)
            {
                return;
            }

            if (!party.QuestItems.HasBitIndex(questItemId))
            {
                WorldQuestItem wqi = status.World.QuestItems.FirstOrDefault(x => x.IdKey == questItemId);

                if (wqi != null)
                {

                    InitialCombatState initialCombatState = new InitialCombatState()
                    {
                        Difficulty = 1.5f,
                        WorldQuestItemId = wqi.IdKey,
                    };
                    _crawlerService.ChangeState(ECrawlerStates.StartCombat, token, initialCombatState);
                    status.MoveIsStopped = true;
                }
                return;
            }

            await Task.CompletedTask;
        }
    }
}


