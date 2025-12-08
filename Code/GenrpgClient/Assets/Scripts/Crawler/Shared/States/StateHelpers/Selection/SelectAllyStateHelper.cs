using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Combat;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Selection
{

    public class SelectAllyStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.SelectAlly;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();
            List<PartyMember> partyMembers = party.GetActiveParty();

            SelectSpellAction spellAction = new SelectSpellAction();

            stateData.Actions.Add(new CrawlerStateAction("Select a party member:\n"));
            for (int m = 0; m < partyMembers.Count; m++)
            {
                PartyMember partyMember = partyMembers[m];
                char c = (char)(Key.A + m);

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

                stateData.Actions.Add(new CrawlerStateAction(char.ToUpper(c) + partyMember.Name, FromChar(c),
                  ECrawlerStates.SelectSpell, extraData: selectAction,

                    pointerEnterAction: ptrEnterAction));
            }

            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.ExploreWorld));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
