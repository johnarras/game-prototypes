
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.StateHelpers.Exploring;
using Genrpg.Shared.UI.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Crawler
{
    public class CrawlerScreen : BaseScreen
    {
        private ICrawlerService _crawlerService = null;

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            PartyData party = _crawlerService.GetParty();
            AddListener<CrawlerStateData>(OnNewStateData);

            _dispatcher.AddListener<CrawlerCharacterScreenData>(OnCrawlerCharacterData, GetToken());

            _dispatcher.Dispatch(new UpdateCrawlerUI());
            _dispatcher.Dispatch(new RefreshPartyStatus());

            await Task.CompletedTask;
        }

        private void OnNewStateData(CrawlerStateData data)
        {
        }

        private void OnCrawlerCharacterData(CrawlerCharacterScreenData data)
        {
            if (_screenService.GetScreen(ScreenNames.CrawlerCharacter) == null)
            {
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.CrawlerCharacter, data));
            }
        }
    }
}


