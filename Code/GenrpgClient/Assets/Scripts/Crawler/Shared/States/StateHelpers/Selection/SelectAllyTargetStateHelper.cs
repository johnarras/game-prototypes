using Assets.Scripts.Crawler.Shared.GameEvents;
using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers.Combat;
using OxDb.SharedGame.Crawler.States.StateHelpers.Selection.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Selection
{
    public class SelectAllyTargetStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.SelectAllyTarget;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            SelectSpellAction selectSpellAction = action.ExtraData as SelectSpellAction;

            SelectAction selectAction = null;

            Action clearAction = () =>
            {
                _dispatcher.Dispatch(new ClearSelectCrawlerUnitActions());
            };

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

            List<PartyMember> partyMembers = party.ActiveParty;

            bool selectingCaster = false;
            ECrawlerStates nextAction = ECrawlerStates.SelectSpell;
            if (selectAction.Member == null)
            {
                partyMembers = partyMembers.Where(x => !_combatService.IsDisabled(x)).OrderBy(x => x.PartySlot).ToList();
                selectingCaster = true;
            }
            else
            {
                nextAction = selectAction.NextState;
            }


            for (int m = 0; m < partyMembers.Count; m++)
            {
                PartyMember partyMember = partyMembers[m];
                char c = (char)('A' + m);

                Action clickAction = null;

                if (selectingCaster)
                {
                    clickAction = delegate ()
                    {
                        selectAction.Member = partyMember;
                        _dispatcher.Dispatch(new ClearSelectCrawlerUnitActions());

                    };
                }
                else // Selecting target.
                {
                    clickAction = delegate ()
                    {
                        selectAction.Action.FinalTargets.Add(partyMember);
                        selectAction.Member.AddAction(selectAction.Action);
                        _dispatcher.Dispatch(new ClearSelectCrawlerUnitActions());
                    };
                }

                CrawlerStateAction clickCrawlerAction = new CrawlerStateAction(char.ToUpper(c) + " " + partyMember.Name, FromChar(c),
                  nextAction, clickAction, action.ExtraData);

                stateData.Actions.Add(clickCrawlerAction);

                Action clickIconAction = () =>
                {
                    clickAction.Invoke();
                    _crawlerService.ChangeState(stateData, clickCrawlerAction, token);
                };

                _dispatcher.Dispatch(new SelectPartyMemberIconAction() { ClickAction = clickIconAction, Member = partyMember });
            }

            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, selectAction.ReturnState, clearAction));


            await Task.CompletedTask;
            return stateData;
        }
    }
}


