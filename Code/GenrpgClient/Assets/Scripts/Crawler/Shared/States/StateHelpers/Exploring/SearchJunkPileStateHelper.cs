using Assets.Scripts.Crawler.Services.CrawlerMaps;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.Exploring
{
    public class SearchJunkPileStateHelper : BaseStateHelper
    {
        private ICrawlerMapService _mapService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.SearchJunkPile;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            if (_gs.Rand.NextDouble() < 0.5f)
            {

            }


            stateData.Actions.Add(new CrawlerStateAction("Yes", Key.Y, ECrawlerStates.SearchJunkPile));
            stateData.Actions.Add(new CrawlerStateAction("No", Key.N, ECrawlerStates.ExploreWorld));

            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.ExploreWorld));

            return stateData;
        }
    }
}
