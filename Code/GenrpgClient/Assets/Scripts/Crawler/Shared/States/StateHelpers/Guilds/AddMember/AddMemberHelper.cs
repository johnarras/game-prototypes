using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.Roles.Constants;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;


namespace OxDb.SharedGame.Crawler.States.StateHelpers.Guilds.AddMember
{
    public class AddMemberHelper : BaseStateHelper
    {
        private IPartyService _partyService = null;
        public override ECrawlerStates HelperKey => ECrawlerStates.AddMember;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            stateData.AddText("Your max party size is " + _partyService.GetMaxPartySize(party));
            for (int m = 0; m < party.InGuild.Count; m++)
            {
                PartyMember member = party.InGuild[m];

                StringBuilder sb = new StringBuilder();
                sb.Append(member.Name);

                List<Role> roles = roleSettings.GetRoles(member.Roles);

                roles = roles.OrderByDescending(x => x.RoleCategoryId).ThenBy(x => -x.IdKey).ToList();
                foreach (Role role in roles)
                {
                    UnitRole unitRole = member.Roles.FirstOrDefault(x => x.RoleId == role.IdKey);

                    if (role.RoleCategoryId == RoleCategories.Class)
                    {
                        sb.Append(" " + role.Name + "(" + unitRole.Level + ")");
                    }
                    else
                    {
                        sb.Append(" " + role.Name + " ");
                    }
                }

                stateData.Actions.Add(new CrawlerStateAction(sb.ToString(), Key.None, ECrawlerStates.AddMember,
                delegate
                {
                    party = _crawlerService.GetParty();
                    _partyService.AddActivePartyMember(party, member);
                    _statService.CalcPartyStats(party, true);
                    _crawlerService.SaveGame();


                }, member, member.PortraitName));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", Key.Escape, ECrawlerStates.GuildMain, null, null));

            await Task.CompletedTask;
            return stateData;
        }
    }
}


