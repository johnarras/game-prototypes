using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Quests.Entities;
using Genrpg.Shared.Crawler.Quests.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.NpcsQuests
{
    public class QuestDetailStateHelper : BaseStateHelper
    {
        private ICrawlerQuestService _questService = null;

        public override ECrawlerStates Key => ECrawlerStates.QuestDetail;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            FullQuest fullQuest = action.ExtraData as FullQuest;

            PartyData party = _crawlerService.GetParty();

            if (fullQuest == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Missing quest data." };
            }

            ECrawlerStates nextState = fullQuest.ReturnState;
            object nextStateAction = fullQuest.NpcDetail;

            stateData.AddText("Quest: " + await _questService.ShowQuestStatus(party, fullQuest.Quest.IdKey, true, true, true));
            
            if (fullQuest.Progress == null)
            {
                stateData.Actions.Add(new CrawlerStateAction("Accept Quest", 'A', nextState,
                    () =>
                    {
                        _questService.AcceptQuest(party, fullQuest, token);
                    }, nextStateAction));
            }
            else
            {
                stateData.Actions.Add(new CrawlerStateAction("Drop Quest", 'D', nextState,
                    () =>
                    {
                        _questService.DropQuest(party, fullQuest, token);
                    }, nextStateAction));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, nextState, null, fullQuest.NpcDetail));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
