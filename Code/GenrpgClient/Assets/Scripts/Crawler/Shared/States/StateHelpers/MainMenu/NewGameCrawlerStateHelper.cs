using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers;
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
