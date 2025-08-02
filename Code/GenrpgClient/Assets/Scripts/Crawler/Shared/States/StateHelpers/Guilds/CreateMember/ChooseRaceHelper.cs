using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Guilds.CreateMember
{
    public class ChooseRaceHelper : BaseStateHelper
    {
        public override ECrawlerStates Key => ECrawlerStates.ChooseRace;


        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();


            PartyData party = _crawlerService.GetParty();

            PartyMember member = new PartyMember()
            {
                Id = party.GetNextId("P"),
                FactionTypeId = FactionTypes.Player,
            };
            

            List<Role> races = _gameData.Get<RoleSettings>(_gs.ch).GetData().Where(x => x.RoleCategoryId == RoleCategories.Origin).ToList();

            foreach (Role race in races)
            {

                stateData.Actions.Add(new CrawlerStateAction(race.Name, CharCodes.None, ECrawlerStates.ChooseClass,
                    delegate
                    {
                        member.Roles.Add(new UnitRole() { RoleId = race.IdKey, Level=1 });
                        member.UnitTypeId = 1;
                    }, member, null, () => { ShowInfo(EntityTypes.Role, race.IdKey); }
                    ));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.GuildMain));

            await Task.CompletedTask;
            return stateData;

        }
    }
}
