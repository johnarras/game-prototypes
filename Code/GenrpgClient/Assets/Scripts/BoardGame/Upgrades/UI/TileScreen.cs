using Assets.Scripts.Entities.UI;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.Tiles.Settings;
using Genrpg.Shared.UI.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.BoardGame.Tiles.UI
{
    public class TileScreen : BaseScreen
    {
        public GText Header;
        public GText Description;

        public GButton UpgradeScreenButton;

        public TileUpgradeUI UpgradeUI;

        private TileTypeWithIndex _tileType = null;
        protected override async Task OnStartOpen(object data, CancellationToken token)
        {

            _tileType = data as TileTypeWithIndex;

            if (_tileType == null || _tileType.TileType == null)
            {
                StartClose();
                return;
            }

            UpgradeUI.SetData(_tileType.TileType.IdKey);

            _uiService.SetText(Header, _tileType.TileType.Name);
            _uiService.SetText(Description, _tileType.TileType.Desc);

            _uiService.SetButton(UpgradeScreenButton, GetName(), OpenUpgradeScreen);

            await Task.CompletedTask;
        }

        private void OpenUpgradeScreen()
        {
            BoardData boardData = _gs.ch.Get<BoardData>();
            if (!boardData.IsOwnBoard())
            {
                return;
            }
            StartClose();
            _screenService.Open(ScreenNames.TileUpgrade);
        }
    }
}
