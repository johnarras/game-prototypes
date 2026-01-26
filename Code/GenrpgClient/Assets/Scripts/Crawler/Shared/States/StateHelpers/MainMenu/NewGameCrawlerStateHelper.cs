using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.MainMenu
{
    public class NewGameCrawlerStateHelper : BaseStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.NewGame;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();


            await Task.CompletedTask;
            return stateData;
        }
    }
}
