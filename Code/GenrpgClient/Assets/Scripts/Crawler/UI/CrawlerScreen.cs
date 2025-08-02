
using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.StateHelpers.Exploring;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.UI.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Crawler
{
    public class CrawlerScreen : BaseScreen
    {
        private ICrawlerService _crawlerService = null;
        private ILootGenService _lootGenService = null;

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            PartyData party = _crawlerService.GetParty();
            AddListener<CrawlerStateData>(OnNewStateData);

            _dispatcher.AddListener<CrawlerCharacterScreenData>(OnCrawlerCharacterData, GetToken());

            _dispatcher.Dispatch(new UpdateCrawlerUI());
            _dispatcher.Dispatch(new RefreshPartyStatus());

            ItemGenArgs igd = new ItemGenArgs() { Level = 10, QualityTypeId = QualityTypes.Uncommon };
            Item item = _lootGenService.GenerateItem(igd);

            await Task.CompletedTask;
        }

        private void OnNewStateData(CrawlerStateData data)
        {
        }

        private void OnCrawlerCharacterData(CrawlerCharacterScreenData data)
        {
            _screenService.Open(ScreenNames.CrawlerCharacter, data);
        }
    }
}
