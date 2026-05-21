using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Roles.Constants;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Stats.Entities;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;


namespace OxDb.SharedGame.Crawler.States.StateHelpers.Guilds.CreateMember
{
    public class ChooseClassHelper : BaseStateHelper
    {

        public override ECrawlerStates HelperKey => ECrawlerStates.ChooseClass;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            PartyMember member = action.ExtraData as PartyMember;

            long totalClasses = 1;

            PartyData party = _crawlerService.GetParty();

            if (!_optionsService.HasOption(party, CrawlerOptions.WholeParty))
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

                if (member.Roles.FastAny(x => x.RoleId == role.IdKey))
                {
                    continue;
                }

                string desc = role.Desc;

                ECrawlerStates nextState = ECrawlerStates.RollStats;
                if (member.Roles.Count < 1 + totalClasses - 1)
                {
                    nextState = ECrawlerStates.ChooseClass;
                }

                stateData.Actions.Add(new CrawlerStateAction(role.Name, Key.None, nextState,
                    delegate
                    {
                        member.Roles.Add(new UnitRole() { RoleId = role.IdKey, Level = 1 });
                    }, member, null, (GameObject go) => { ShowInfo(EntityTypes.Role, role.IdKey); }

                    ));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", Key.Escape, ECrawlerStates.ChooseRace,
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


