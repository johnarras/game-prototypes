using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.LoadSave.Services;
using Genrpg.Shared.UI.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.MainMenu
{
    public class CrawlerMainMenuScreen : MainMenuScreen
    {
        public GButton ContinueGameButton;
        public GButton LoadGameButton;
        public GButton NewCrawlerGameButton;
        public GButton CloseButton;

        protected IInputService _inputService = null;
        private ILoadSaveService _loadSaveService = null;
        private ICrawlerService _crawlerService = null;
        private IClientAppService _clientAppService = null;

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await base.OnStartOpen(data, token);

            _uiService.SetButton(ContinueGameButton, GetName(), ClickContinue);
            _uiService.SetButton(QuitGameButton, GetName(), ClickQuit);
            _uiService.SetButton(NewCrawlerGameButton, GetName(), ClickNewCrawler);
            _uiService.SetButton(LoadGameButton, GetName(), ClickLoadGame);


            if (!_loadSaveService.HaveCurrentGame<PartyData>())
            {
                _uiService.SetInteractable(ContinueGameButton, false);
            }

            if (_screenService.GetScreen(_crawlerService.GetCrawlerScreenId()) == null)
            {
                _clientEntityService.SetActive(CloseButton, false);
            }
        }

        protected override void ScreenUpdate()
        {
            base.ScreenUpdate();

            if (_inputService.ContinueKeyIsDown() &&
                _screenService.GetScreen(ScreenNames.Crawler) != null)
            {
                _screenService.Close(ScreenNames.CrawlerMainMenu);
            }
        }

        private void ClickNewCrawler()
        {
            StartClose();
            _screenService.Open(ScreenNames.CrawlerNewGameOptions);
        }

        private void ClickContinue()
        {
            _crawlerService.ContinueGame();
        }

        private void ClickLoadGame()
        {
            _screenService.CloseAll();
            _screenService.Open(ScreenNames.LoadSave);
        }

        private void ClickQuit()
        {
            _clientAppService.Quit();
        }

        private void ClickToggleFullScreen()
        {
            _clientAppService.SetFullScreen(!_clientAppService.IsFullScreen());
        }
    }
}
