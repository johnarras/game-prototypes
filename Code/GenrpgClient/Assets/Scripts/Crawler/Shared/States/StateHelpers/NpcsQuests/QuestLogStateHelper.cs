using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Quests.Entities;
using Genrpg.Shared.Crawler.Quests.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Worlds.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;


namespace Genrpg.Shared.Crawler.States.StateHelpers.NpcsQuests
{
    public class QuestLogStateHelper : BaseStateHelper
    {

        private ICrawlerQuestService _questService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.QuestLog;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            List<PartyQuest> partyQuests = party.Quests.OrderBy(x => x.CrawlerQuestId).ToList();

            stateData.AddText("Your Quests:");
            stateData.AddBlankLine();


            foreach (PartyQuest partyQuest in partyQuests)
            {
                CrawlerQuest quest = world.GetQuest(partyQuest.CrawlerQuestId);

                if (quest == null)
                {
                    party.Quests.Remove(partyQuest);
                    continue;
                }
                FullQuest fullQuest = new FullQuest()
                {
                    Quest = quest,
                    Progress = partyQuest,
                    ReturnState = ECrawlerStates.QuestLog,
                };

                stateData.Actions.Add(new CrawlerStateAction
                    (
                        await _questService.ShowQuestStatus(party, quest.IdKey, false, true, false),
                        Key.None, ECrawlerStates.QuestDetail, null, fullQuest
                        ));
            }

            stateData.Actions.Add(new CrawlerStateAction("Back to the city", Key.Escape, ECrawlerStates.ExploreWorld));

            await Task.CompletedTask;
            return stateData;
        }
    }
}
