using Assets.Scripts.Crawler.ClientEvents.WorldPanelEvents;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Units.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Guilds.CreateMember
{
    public class ChoosePortraitHelper : BaseStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.ChoosePortrait;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            IReadOnlyList<UnitType> allUnitTypes = _gameData.Get<UnitTypeSettings>(null).GetData();

            PartyData party = _crawlerService.GetParty();

            PartyMember member = action.ExtraData as PartyMember;

            allUnitTypes = allUnitTypes.OrderBy(x => x.Name).ToList();

            foreach (UnitType unitType in allUnitTypes)
            {
                if (unitType.IdKey < 1 || string.IsNullOrEmpty(unitType.Icon))
                {
                    continue;
                }
                stateData.Actions.Add(new CrawlerStateAction(unitType.Name, Key.None, ECrawlerStates.ChooseName,
                   delegate
                   {
                       member.PortraitName = unitType.Icon;
                   }, member, unitType.Icon,
                   (GameObject go) => { _dispatcher.Dispatch(new SetWorldPicture(unitType.Icon, false)); }
                   )
                   );
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", Key.Escape, ECrawlerStates.ChooseClass,
                delegate { member.PortraitName = null; }, member));


            await Task.CompletedTask;

            return stateData;
        }
    }
}
