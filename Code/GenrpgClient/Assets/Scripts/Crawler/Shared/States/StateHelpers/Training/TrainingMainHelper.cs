using OxDb.SharedGame.Buildings.Constants;
using OxDb.SharedGame.Crawler.Constants;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers.Buildings;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;


namespace OxDb.SharedGame.Crawler.States.StateHelpers.Training
{
    public class TrainingMainHelper : BuildingStateHelper
    {

        public override ECrawlerStates HelperKey => ECrawlerStates.TrainingMain;
        public override long TriggerBuildingId() { return BuildingTypes.Trainer; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            stateData.BGSpriteName = CrawlerClientConstants.BuildingImage;
            PartyData party = _crawlerService.GetParty();

            stateData.AddText("Welcome to the trainer. What would you like to do?");


            stateData.Actions.Add(new CrawlerStateAction("Train Levels:", Key.T, ECrawlerStates.TrainingLevelSelect));
            stateData.Actions.Add(new CrawlerStateAction("Add a Class:", Key.A, ECrawlerStates.TrainingClassSelect));

            if (_optionsService.HasOption(party, CrawlerOptions.MemberUpgrades))
            {
                stateData.Actions.Add(new CrawlerStateAction("Upgrade Training:", Key.U, ECrawlerStates.TrainingUpgradeSelect));
            }

            stateData.Actions.Add(new CrawlerStateAction("Back to the city", Key.Escape, ECrawlerStates.ExploreWorld));

            await Task.CompletedTask;
            return stateData;
        }
    }
}


