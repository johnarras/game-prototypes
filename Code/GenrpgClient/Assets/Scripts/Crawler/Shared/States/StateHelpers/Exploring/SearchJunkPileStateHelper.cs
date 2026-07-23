using OxDb.Client.Crawler.Encounters.Services;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Exploring
{

    public class SearchJunkPileStateHelper : BaseStateHelper
    {
        private IMapEncounterService _mapEncounterService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.SearchJunkPile;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            stateData.AddText("There is a junk pile here.");
            stateData.AddText("Would you like to search it?");

            stateData.Actions.Add(new CrawlerStateAction("Yes", Key.Y, ECrawlerStates.ExploreWorld,
                () =>
                {
                    _ = _mapEncounterService.SearchJunkPile(token);
                }));
            stateData.Actions.Add(new CrawlerStateAction("No", Key.N, ECrawlerStates.ExploreWorld));

            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.ExploreWorld));

            await Task.CompletedTask;
            return stateData;
        }
    }
}


