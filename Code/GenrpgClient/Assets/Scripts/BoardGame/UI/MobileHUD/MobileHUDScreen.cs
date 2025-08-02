
using Assets.Scripts.BoardGame.Controllers;
using Genrpg.Shared.BoardGame.Upgrades.Services;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Users.PlayerData;
using Newtonsoft.Json.Bson;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.MobileHUD
{
    public class MobileHUDScreen : BaseScreen
    {

        private IBoardGameController _boardGameController;
        private IUpgradeBoardService _upgradeBoardService;

        public GButton RollButton;
        public GButton MarkerScreenButton;
        public GButton ResetButton;

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await Task.CompletedTask;

            _uiService.SetButton(RollButton, GetName(), RollDice);
            _uiService.SetButton(MarkerScreenButton, GetName(), ShowMarkerScreen);
            _uiService.SetButton(ResetButton, GetName(), ResetGame);

        }

        private void RollDice()
        {
            _boardGameController.RollDice();
        }

        private void ShowMarkerScreen()
        {
            _screenService.Open(ScreenNames.Marker);
        }
        
        private void ResetGame()
        {
            _initClient.FullResetGame();
        }
    }
}
