using OxDb.Client.ClientEvents.UI;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.SharedGame.Crawler.Options.Settings;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.UI.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.UI.MainMenu
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

                if (option.ForceDefault)
                {
                    continue;
                }

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
            _ = _crawlerService.NewGamePhaseThree(options);
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


