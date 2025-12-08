using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Combat;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Selection
{
    public class SelectAllyTargetStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.SelectAllyTarget;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            SelectSpellAction selectSpellAction = action.ExtraData as SelectSpellAction;

            SelectAction selectAction = null;

            if (selectSpellAction != null)
            {
                selectAction = selectSpellAction.Action;
            }
            else
            {
                selectAction = action.ExtraData as SelectAction;
            }

            if (selectAction == null ||
                selectAction.Action == null ||
                selectAction.Member == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Cannot select ally without select action" };
            }

            List<PartyMember> partyMembers = party.GetActiveParty();

            bool selectingCaster = false;
            ECrawlerStates nextAction = ECrawlerStates.SelectSpell;
            if (selectAction.Member == null)
            {
                partyMembers = partyMembers.Where(x => !_combatService.IsDisabled(x)).ToList();
                selectingCaster = true;
            }
            else
            {
                nextAction = selectAction.NextState;
            }


            for (int m = 0; m < partyMembers.Count; m++)
            {
                PartyMember partyMember = partyMembers[m];
                char c = (char)(Key.A + m);

                Action clickAction = null;

                if (selectingCaster)
                {
                    clickAction = delegate ()
                    {
                        selectAction.Member = partyMember;

                    };
                }
                else // Selecting target.
                {
                    clickAction = delegate ()
                    {
                        selectAction.Action.FinalTargets.Add(partyMember);
                        selectAction.Member.AddAction(selectAction.Action);
                    };
                }

                stateData.Actions.Add(new CrawlerStateAction(char.ToUpper(c) + " " + partyMember.Name, FromChar(c),
                  nextAction, clickAction, action.ExtraData));
            }

            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, selectAction.ReturnState));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
