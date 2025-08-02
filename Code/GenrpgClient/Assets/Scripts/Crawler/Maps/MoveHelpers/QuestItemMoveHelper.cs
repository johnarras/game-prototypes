using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public class QuestItemMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 275;

        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            if (!status.MovedPosition)
            {
                return;
            }

            long questItemId = status.MapRoot.Map.GetEntityId(status.EX,status.EZ, EntityTypes.QuestItem);

            if (questItemId < 1)
            {
                return;
            }

            if (!party.QuestItems.HasBit(questItemId))
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
                    status.MoveIsComplete = true;
                }
                return;
            }

            await Task.CompletedTask;
        }
    }
}
