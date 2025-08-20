using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Training
{
    public class TrainingMainHelper : BuildingStateHelper
    {

        public override ECrawlerStates Key => ECrawlerStates.TrainingMain;
        public override long TriggerBuildingId() { return BuildingTypes.Trainer; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            stateData.BGSpriteName = CrawlerClientConstants.TrainerImage;
            PartyData party = _crawlerService.GetParty();

            stateData.AddText("Welcome to the trainer. What would you like to do?");


            stateData.Actions.Add(new CrawlerStateAction("Train Levels:", 'T', ECrawlerStates.TrainingLevelSelect));
            stateData.Actions.Add(new CrawlerStateAction("Add a Class:", 'A', ECrawlerStates.TrainingClassSelect));
            stateData.Actions.Add(new CrawlerStateAction("Upgrade Training:", 'U', ECrawlerStates.TrainingUpgradeSelect));

            stateData.Actions.Add(new CrawlerStateAction("Back to the city", CharCodes.Escape, ECrawlerStates.ExploreWorld));

            await Task.CompletedTask;
            return stateData;
        }
    }
}
