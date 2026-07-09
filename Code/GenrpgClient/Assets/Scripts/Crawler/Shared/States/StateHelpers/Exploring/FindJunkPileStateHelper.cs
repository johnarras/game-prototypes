using Assets.Scripts.Crawler.Services.CrawlerMaps;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Exploring
{

    public class FindJunkPileStateHelper : BaseStateHelper
    {

        private ICrawlerMapService _mapService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.FindJunkPile;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            stateData.AddText("There is a junk pile here.");
            stateData.AddText("Would you like to search it?");

            stateData.Actions.Add(new CrawlerStateAction("Yes", Key.Y, ECrawlerStates.SearchJunkPile));
            stateData.Actions.Add(new CrawlerStateAction("No", Key.N, ECrawlerStates.ExploreWorld));

            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.ExploreWorld));

            return stateData;
        }
    }
}


