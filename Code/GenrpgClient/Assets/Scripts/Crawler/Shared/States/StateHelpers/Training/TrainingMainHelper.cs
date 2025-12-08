using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Options.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Training
{
    public class TrainingMainHelper : BuildingStateHelper
    {

        public override ECrawlerStates HelperKey => ECrawlerStates.TrainingMain;
        public override long TriggerBuildingId() { return BuildingTypes.Trainer; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            stateData.BGSpriteName = CrawlerClientConstants.TrainerImage;
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
