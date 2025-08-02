using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.Crawler.States.StateHelpers.Training;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.Training
{
    public abstract class BaseTrainingSelectMemberHelper : BaseStateHelper
    {
        public abstract override ECrawlerStates Key { get; }

        public abstract string GetMainMessage();

        public abstract ECrawlerStates GetNextState();

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            stateData.AddText(GetMainMessage());

            foreach (PartyMember member in party.GetActiveParty())
            {

                ECrawlerStates nextState = GetNextState();
                char nextKeyCode = (char)(member.PartySlot + '0');
                if (_combatService.IsDisabled(member))
                {
                    nextState = ECrawlerStates.None;
                    nextKeyCode = CharCodes.None;
                }

                stateData.Actions.Add(new CrawlerStateAction(member.PartySlot + " " + member.Name, nextKeyCode, nextState, extraData: new TrainingMemberData() { Member = member}));
            }

            stateData.Actions.Add(new CrawlerStateAction("Back to the trainer", CharCodes.Escape, ECrawlerStates.TrainingMain));

            await Task.CompletedTask;
            return stateData;

        }
    }
}
