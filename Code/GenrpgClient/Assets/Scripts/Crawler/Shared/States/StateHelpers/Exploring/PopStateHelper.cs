using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Exploring
{
    public class PopStateHelper : BaseStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.PopState;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            stateData.DoNotTransitionToThisState = true;

            _crawlerService.PopState();

            await Task.CompletedTask;
            return stateData;
        }
    }
}


