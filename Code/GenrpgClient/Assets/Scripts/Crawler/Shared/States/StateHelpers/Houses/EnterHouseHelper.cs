using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Houses
{
    public class EnterHouseHelper : BuildingStateHelper
    { 
        public override ECrawlerStates Key => ECrawlerStates.EnterHouse;
        public override long TriggerBuildingId() { return BuildingTypes.House; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();


            PartyData party = _crawlerService.GetParty();

            stateData.BGSpriteName = CrawlerClientConstants.HouseImage + GetBuildingImageIndex(party, TriggerBuildingId());


            if (_rand.NextDouble() < 0.3f)
            {
                stateData = new CrawlerStateData(ECrawlerStates.StartCombat, true)
                {
                };
            }
            else
            {
                if (party.Combat == null)
                {
                    stateData.Actions.Add(new CrawlerStateAction("Exit House", CharCodes.Escape, ECrawlerStates.ExploreWorld));
                    AddSpaceAction(stateData);
                }
            }
            await Task.CompletedTask;
            return stateData;
        }
    }
}
