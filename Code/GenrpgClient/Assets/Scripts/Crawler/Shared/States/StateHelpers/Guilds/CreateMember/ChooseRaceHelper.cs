using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Roles.Constants;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Factions.Constants;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Guilds.CreateMember
{
    public class ChooseRaceHelper : BaseStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.ChooseRace;


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

                stateData.Actions.Add(new CrawlerStateAction(race.Name, Key.None, ECrawlerStates.ChooseClass,
                    delegate
                    {
                        member.Roles.Add(new UnitRole() { RoleId = race.IdKey, Level = 1 });
                        member.UnitTypeId = 1;
                    }, member, null, (GameObject go) => { ShowInfo(EntityTypes.Role, race.IdKey); }
                    ));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", Key.Escape, ECrawlerStates.GuildMain));

            await Task.CompletedTask;
            return stateData;

        }
    }
}


