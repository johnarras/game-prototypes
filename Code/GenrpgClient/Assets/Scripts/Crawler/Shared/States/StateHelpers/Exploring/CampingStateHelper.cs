using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.Crawler.TimeOfDay.Constants;
using Genrpg.Shared.Crawler.TimeOfDay.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.Exploring
{
    public class CampingStateHelper : BaseStateHelper
    {

        private ITimeOfDayService _timeService = null;

        public override ECrawlerStates Key => ECrawlerStates.Camping;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
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
