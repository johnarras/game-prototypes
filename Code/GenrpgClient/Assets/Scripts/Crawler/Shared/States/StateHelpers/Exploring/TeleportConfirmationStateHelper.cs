using Assets.Scripts.Crawler.Maps.Services.Entities;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.Entities.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.Exploring
{
    public class TeleportConfirmationStateHelper : BaseStateHelper
    {
        private ICrawlerMapService _mapService = null;

        public override ECrawlerStates Key => ECrawlerStates.TeleportConfirmation;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();
            MapCellDetail detail = action.ExtraData as MapCellDetail;

            if (detail == null || detail.EntityTypeId != EntityTypes.TeleportIn)
            {
                _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token);
                return stateData;
            }

            stateData.Actions.Add(new CrawlerStateAction("There is a teleport here."));
            stateData.Actions.Add(new CrawlerStateAction("Do you wish to enter it?"));
            stateData.Actions.Add(new CrawlerStateAction("Yes", 'Y', ECrawlerStates.ExploreWorld,
            () =>
            {
                _mapService.MovePartyTo(party, detail.ToX, detail.ToZ, party.CurrPos.Rot, true, token);
            }, null));

            stateData.Actions.Add(new CrawlerStateAction("No", 'N', ECrawlerStates.ExploreWorld));
            await Task.CompletedTask;
            return stateData;
        }
    }
}
