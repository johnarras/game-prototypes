using OxDb.Client.Crawler.Shared.GameEvents;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Roles.Constants;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers.Combat;
using OxDb.SharedGame.Crawler.States.StateHelpers.Selection.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Selection
{

    public class SelectAllyStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.SelectAlly;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();
            List<PartyMember> partyMembers = party.ActiveParty.OrderBy(x => x.PartySlot).ToList();

            SelectSpellAction spellAction = new SelectSpellAction();


            Action clearAction = () =>
            {
                _dispatcher.Dispatch(new ClearSelectCrawlerUnitActions());
            };

            stateData.Actions.Add(new CrawlerStateAction("Select a party member:\n"));
            for (int m = 0; m < partyMembers.Count; m++)
            {
                PartyMember partyMember = partyMembers[m];
                char c = (char)('A' + m);

                SelectAction selectAction = new SelectAction()
                {
                    Member = partyMember,
                    ReturnState = ECrawlerStates.SelectAlly,
                    NextState = ECrawlerStates.WorldCast,
                };

                Role classRole = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(partyMember.Roles).FirstOrDefault(x => x.RoleCategoryId == RoleCategories.Class);

                Action<GameObject> ptrEnterAction = null;

                if (classRole != null)
                {
                    ptrEnterAction = (GameObject go) => { ShowInfo(EntityTypes.Role, classRole.IdKey); };
                }
                CrawlerStateAction newAction = new CrawlerStateAction(char.ToUpper(c) + partyMember.Name, FromChar(c),
                  ECrawlerStates.SelectSpell, clearAction,

                  extraData: selectAction, pointerEnterAction: ptrEnterAction);
                stateData.Actions.Add(newAction);

                Action clickAction = () =>
                {
                    _crawlerService.ChangeState(stateData, newAction, token);
                    _dispatcher.Dispatch(new ClearSelectCrawlerUnitActions());
                };

                _dispatcher.Dispatch(new SelectPartyMemberIconAction() { Member = partyMember, ClickAction = clickAction });

            }

            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.ExploreWorld, clearAction));


            await Task.CompletedTask;
            return stateData;
        }
    }
}


