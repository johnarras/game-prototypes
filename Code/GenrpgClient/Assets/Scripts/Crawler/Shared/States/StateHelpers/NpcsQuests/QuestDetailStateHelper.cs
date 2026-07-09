using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Quests.Entities;
using OxDb.SharedGame.Crawler.Quests.Services;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.NpcsQuests
{
    public class QuestDetailStateHelper : BaseStateHelper
    {
        private ICrawlerQuestService _questService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.QuestDetail;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
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
                stateData.Actions.Add(new CrawlerStateAction("Accept Quest", Key.A, nextState,
                    () =>
                    {
                        _questService.AcceptQuest(party, fullQuest, token);
                    }, nextStateAction));
            }
            else
            {
                stateData.Actions.Add(new CrawlerStateAction("Drop Quest", Key.D, nextState,
                    () =>
                    {
                        _questService.DropQuest(party, fullQuest, token);
                    }, nextStateAction));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", Key.Escape, nextState, null, fullQuest.NpcDetail));


            await Task.CompletedTask;
            return stateData;
        }
    }
}


