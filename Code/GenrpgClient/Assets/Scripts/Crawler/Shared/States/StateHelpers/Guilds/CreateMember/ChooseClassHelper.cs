using Genrpg.Shared.Crawler.Modes.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Guilds.CreateMember
{
    public class ChooseClassHelper : BaseStateHelper
    {

        private ICrawlerModeService _modeService = null;

        public override ECrawlerStates Key => ECrawlerStates.ChooseClass;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            PartyMember member = action.ExtraData as PartyMember;

            long totalClasses = 1;

            PartyData party = _crawlerService.GetParty();

            if (_modeService.SinglePartyMember(party.Mode))
            {
                totalClasses = 2;
            }

            IReadOnlyList<Role> roles = _gameData.Get<RoleSettings>(null).GetData().Where(x => x.RoleCategoryId == RoleCategories.Class).ToList();

            foreach (Role role in roles)
            {
                if (role.IdKey < 1)
                {
                    continue;
                }

                if (member.Roles.Any(x => x.RoleId == role.IdKey))
                {
                    continue;
                }

                string desc = role.Desc;

                ECrawlerStates nextState = ECrawlerStates.RollStats;
                if (member.Roles.Count < 1 + totalClasses - 1)
                {
                    nextState = ECrawlerStates.ChooseClass;
                }

                stateData.Actions.Add(new CrawlerStateAction(role.Name, CharCodes.None, nextState,
                    delegate
                    {
                        member.Roles.Add(new UnitRole() { RoleId = role.IdKey, Level = 1 });
                    }, member, null, (GameObject go) => { ShowInfo(EntityTypes.Role, role.IdKey); }

                    ));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.ChooseRace,
                delegate
                {
                    member.Stats = new StatGroup();
                    while (member.Roles.Count > 1)
                    {
                        member.Roles.RemoveAt(1);
                    }
                },
                extraData: member));
            await Task.CompletedTask;
            return stateData;

        }
    }
}
