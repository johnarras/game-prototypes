using OxDb.Client.Audio.ClientEvents;
using OxDb.Client.Crawler.Constants;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.Client.Crawler.Shared.States.StateHelpers.Exploring
{
    public class TeleportConfirmationStateHelper : BaseStateHelper
    {
        private ICrawlerMapService _mapService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.TeleportConfirmation;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
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
            stateData.Actions.Add(new CrawlerStateAction("Yes", Key.Y, ECrawlerStates.ExploreWorld,
            () =>
            {
                _dispatcher.Dispatch(new PlaySound(CrawlerAudio.TeleportActivate));
                _mapService.MovePartyTo(party, detail.ToX, detail.ToZ, party.CurrPos.Rot, true, token);
            }, null));

            stateData.Actions.Add(new CrawlerStateAction("No", Key.N, ECrawlerStates.ExploreWorld));
            await Task.CompletedTask;
            return stateData;
        }
    }
}


