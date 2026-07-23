using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers;
using OxDb.SharedGame.Crawler.TimeOfDay.Constants;
using OxDb.SharedGame.Crawler.TimeOfDay.Services;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Shared.States.StateHelpers.Exploring
{
    public class CampingStateHelper : BaseStateHelper
    {

        private ITimeOfDayService _timeService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.Camping;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();


            PartyData party = _crawlerService.GetParty();
            _statService.FullyRestParty(party);

            await _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.Rest);

            _dispatcher.Dispatch(new UpdateCrawlerUI());

            stateData.AddText("You are fully rested.");
            stateData.AddBlankLine();
            AddSpaceAction(stateData);
            return stateData;
        }
    }
}


