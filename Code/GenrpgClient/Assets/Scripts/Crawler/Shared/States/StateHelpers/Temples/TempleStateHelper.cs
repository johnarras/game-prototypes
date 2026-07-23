using OxDb.Client.UI.Constants;
using OxDb.SharedGame.Buildings.Constants;
using OxDb.SharedGame.Crawler.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers.Buildings;
using OxDb.SharedGame.Crawler.Temples.Services;
using OxDb.SharedGame.Currencies.Constants;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Temples
{
    public class TempleStateHelper : BuildingStateHelper
    {

        private ITempleService _templeService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.Temple;
        public override long TriggerBuildingId() { return BuildingTypes.Temple; }

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            stateData.BGSpriteName = CrawlerClientConstants.BuildingImage;

            TempleResult result = action.ExtraData as TempleResult;

            if (result != null)
            {
                string color = result.Success ? TextColors.ColorYellow : TextColors.ColorRed;

                stateData.Actions.Add(new CrawlerStateAction(_textService.HighlightText(result.Message, color)));
            }

            stateData.AddText("Party Gold: " + party.Currencies[CoreCurrencyTypes.Coins]);

            foreach (PartyMember member in party.ActiveParty)
            {
                long cost = _templeService.GetHealingCostForMember(party, member);
                if (cost > 0)
                {
                    TempleResult newResult = new TempleResult();
                    stateData.Actions.Add(new CrawlerStateAction(member.Name + "(" + cost + ")", Key.None, ECrawlerStates.Temple,
                        () =>
                        {
                            _templeService.HealPartyMember(party, member, newResult);
                        }, forceButton: false, extraData: newResult));
                }
            }


            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.ExploreWorld));
            await Task.CompletedTask;
            return stateData;

        }
    }
}


