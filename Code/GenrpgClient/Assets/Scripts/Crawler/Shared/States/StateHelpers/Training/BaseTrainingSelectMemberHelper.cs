using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers;
using OxDb.SharedGame.Crawler.States.StateHelpers.Training;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.Training
{
    public abstract class BaseTrainingSelectMemberHelper : BaseStateHelper
    {
        public abstract override ECrawlerStates HelperKey { get; }

        public abstract string GetMainMessage();

        public abstract ECrawlerStates GetNextState();

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            stateData.AddText(GetMainMessage());

            foreach (PartyMember member in party.ActiveParty)
            {

                ECrawlerStates nextState = GetNextState();
                Key nextKeyCode = FromChar((char)(member.PartySlot + '0'));
                if (_combatService.IsDisabled(member))
                {
                    nextState = ECrawlerStates.None;
                    nextKeyCode = Key.None;
                }

                stateData.Actions.Add(new CrawlerStateAction(member.PartySlot + " " + member.Name, nextKeyCode, nextState, extraData: new TrainingMemberData() { Member = member }));
            }

            stateData.Actions.Add(new CrawlerStateAction("Back to the trainer", Key.Escape, ECrawlerStates.TrainingMain));

            await Task.CompletedTask;
            return stateData;

        }
    }
}


