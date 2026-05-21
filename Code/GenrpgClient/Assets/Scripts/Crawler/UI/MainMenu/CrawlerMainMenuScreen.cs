using Assets.Scripts.ClientEvents.UI;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.LoadSave.Services;
using OxDb.SharedGame.UI.Constants;
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
                _dispatcher.Dispatch(new CloseScreen(ScreenNames.CrawlerMainMenu));
            }
        }

        private void ClickNewCrawler()
        {
            StartClose();
            _crawlerService.NewGamePhaseOne();
        }

        private void ClickContinue()
        {
            _crawlerService.ContinueGame();
        }

        private void ClickLoadGame()
        {
            _dispatcher.Dispatch(new CloseAllScreens());
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.LoadSave));
        }

        private void ClickQuit()
        {
            _clientAppService.Quit();
        }
    }
}


