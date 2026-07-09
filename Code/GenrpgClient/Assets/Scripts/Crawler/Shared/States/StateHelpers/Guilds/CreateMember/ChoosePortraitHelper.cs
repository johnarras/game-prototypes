using Assets.Scripts.UI.Crawler.CrawlerPanels;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Portraits.Settings;
using OxDb.SharedGame.Portraits.Utils;
using OxDb.SharedGame.Units.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Guilds.CreateMember
{
    public class ChoosePortraitHelper : BaseStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.ChoosePortrait;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PortraitSettings settings = _gameData.Get<PortraitSettings>(null);

            PartyMember member = action.ExtraData as PartyMember;

            for (int p = 1; p <= settings.PortraitCount; p++)
            {
                string suffix = PortraitUtils.GetFileSuffixFromIndex(p);

                string filename = "Portrait" + suffix;

                AddPortraitOption(stateData, member, filename);
            }

            IReadOnlyList<UnitType> allUnitTypes = _gameData.Get<UnitTypeSettings>(null).GetData();

            PartyData party = _crawlerService.GetParty();

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

                   (GameObject go) =>
                   {
                       _dispatcher.Dispatch(new ShowWorldPanelImage(unitType.Icon));
                   }

                   ));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", Key.Escape, ECrawlerStates.ChooseClass,
                delegate { member.PortraitName = null; }, member));


            await Task.CompletedTask;

            return stateData;
        }

        private void AddPortraitOption(CrawlerStateData stateData, PartyMember member, string filename)
        {

            stateData.Actions.Add(new CrawlerStateAction(filename, Key.None, ECrawlerStates.ChooseName,
               delegate
               {
                   member.PortraitName = filename;
               }, member, filename
               // , (GameObject go) => { _dispatcher.Dispatch(new SetWorldPicture(filename, false)); }
               )
               );
        }
    }
}


