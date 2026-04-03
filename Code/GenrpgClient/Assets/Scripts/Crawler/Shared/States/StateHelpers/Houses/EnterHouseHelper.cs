using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Houses
{
    public class EnterHouseHelper : BuildingStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.EnterHouse;
        public override long TriggerBuildingId() { return BuildingTypes.House; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();


            PartyData party = _crawlerService.GetParty();

            stateData.BGSpriteName = CrawlerClientConstants.BuildingImage;

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
                    stateData.Actions.Add(new CrawlerStateAction("Exit House", Key.Escape, ECrawlerStates.ExploreWorld));
                    AddSpaceAction(stateData);
                }
            }
            await Task.CompletedTask;
            return stateData;
        }
    }
}


