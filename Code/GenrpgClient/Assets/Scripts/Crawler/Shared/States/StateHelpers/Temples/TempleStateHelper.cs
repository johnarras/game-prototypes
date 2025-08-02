using Assets.Scripts.UI.Constants;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;
using Genrpg.Shared.Crawler.Temples.Services;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Temples
{
    public class TempleStateHelper : BuildingStateHelper
    {

        private ITempleService _templeService = null;

        public override ECrawlerStates Key => ECrawlerStates.Temple;
        public override long TriggerBuildingId() { return BuildingTypes.Temple; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            stateData.BGSpriteName = CrawlerClientConstants.TempleImage;

            TempleResult result = action.ExtraData as TempleResult;

            if (result != null)
            {
                string color = result.Success ? TextColors.ColorYellow : TextColors.ColorRed;

                stateData.Actions.Add(new CrawlerStateAction(_textService.HighlightText(result.Message, color)));
            }

            stateData.AddText("Party Gold: " + party.Gold);

            foreach (PartyMember member in party.GetActiveParty())
            {
                long cost = _templeService.GetHealingCostForMember(party, member);
                if (cost > 0)
                {
                    TempleResult newResult = new TempleResult();
                    stateData.Actions.Add(new CrawlerStateAction(member.Name + "(" + cost + ")", CharCodes.None, ECrawlerStates.Temple,
                        () =>
                        {
                            _templeService.HealPartyMember(party, member, newResult);
                        }, forceButton: false, extraData: newResult));
                }
            }


            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.ExploreWorld));
            await Task.CompletedTask;
            return stateData;

        }
    }
}
