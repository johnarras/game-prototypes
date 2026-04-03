using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class PopStateHelper : BaseStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.PopState;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            stateData.DoNotTransitionToThisState = true;

            _crawlerService.PopState();

            await Task.CompletedTask;
            return stateData;
        }
    }
}


