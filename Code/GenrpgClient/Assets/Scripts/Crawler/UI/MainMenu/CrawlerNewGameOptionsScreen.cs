using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.ScreenSystem;
using Genrpg.Shared.Crawler.Options.Settings;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.UI.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.MainMenu
{
    public class CrawlerNewGameOptionsScreen : BaseScreen
    {
        private ICrawlerService _crawlerService = null;
        private ICrawlerMapService _mapService = null;

        public GButton NewGameButton;
        public GameObject Anchor;

        public NewGameOptionRow RowPrefab;

        private List<NewGameOptionRow> _rows = new List<NewGameOptionRow>();

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {

            _mapService.CleanMap();
            IReadOnlyList<CrawlerOption> options = _gameData.Get<CrawlerOptionSettings>(_gs.ch).GetData();

            foreach (CrawlerOption option in options)
            {
                NewGameOptionRow newRow = _clientEntityService.FullInstantiate<NewGameOptionRow>(RowPrefab);

                _clientEntityService.AddToParent(newRow, Anchor);

                _rows.Add(newRow);

                _uiService.SetButton(NewGameButton, GetName(), CreateNewGame);

                newRow.Init(option);
            }



            await Task.CompletedTask;
        }

        bool _didStartGame = false;
        public void CreateNewGame()
        {
            int options = 0;

            foreach (NewGameOptionRow row in _rows)
            {
                options |= ((row.IsOptionSet() ? 1 : 0) << (int)row.GetOptionId());
            }
            _didStartGame = true;
            _awaitableService.ForgetAwaitable(_crawlerService.NewGamePhaseThree(options));
            StartClose();
        }

        protected override void OnStartClose()
        {
            if (!_didStartGame)
            {
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.CrawlerMainMenu));
            }
        }
    }
}


