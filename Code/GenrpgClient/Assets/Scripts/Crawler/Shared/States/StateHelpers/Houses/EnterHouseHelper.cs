using OxDb.SharedGame.Buildings.Constants;
using OxDb.SharedGame.Crawler.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers.Buildings;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Houses
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

            if (_rand.Rand.NextDouble() < 0.3f)
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


