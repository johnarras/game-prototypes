using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Upgrades.Services;
using Genrpg.Shared.BoardGame.Upgrades.WebApi;
using Genrpg.Shared.Tiles.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Tiles.UI
{
    public class TileUpgradeScreen : BaseScreen
    {
        private IUpgradeBoardService _upgradeBoardService = null!;

        public GameObject RowAnchor;
        public TileUpgradeUI UpgradeUI;
        public GText Header;

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {

            _dispatcher.AddListener<UpgradeBoardResponse>(OnUpgradeBoard, GetToken());
            BoardData boardData = _gs.ch.Get<BoardData>();

            if (!boardData.IsOwnBoard())
            {
                StartClose();
                return;
            }

            IReadOnlyList<TileType> tileTypes = _gameData.Get<TileTypeSettings>(_gs.user).GetData();

            _clientEntityService.DestroyAllChildren(RowAnchor);

            List<TileType> foundTileTypes = new List<TileType>();

            for (int i = 0; i < boardData.Tiles.GetLength(); i++)
            {
                TileType tileType = tileTypes.FirstOrDefault(x => x.IdKey == boardData.Tiles.Get(i));

                if (tileType == null || !tileType.CanUpgrade() || foundTileTypes.Contains(tileType))
                {
                    continue;
                }

                foundTileTypes.Add(tileType);
            }

            foundTileTypes = foundTileTypes.OrderBy(x=>x.Name).ToList();

            foreach (TileType tileType in foundTileTypes)
            {
                TileUpgradeUI upgradeUI = _clientEntityService.FullInstantiate(UpgradeUI);

                upgradeUI.SetData(tileType.IdKey);

                _clientEntityService.AddToParent(upgradeUI, RowAnchor);
            }

            ShowUpgradeCounts();
            await Task.CompletedTask;
        }

        private void OnUpgradeBoard(UpgradeBoardResponse  response)
        {
            ShowUpgradeCounts();
        }

        private void ShowUpgradeCounts()
        {
            BoardData boardData = _gs.ch.Get<BoardData>();
            if (!boardData.IsOwnBoard())
            {
                return;
            }

            UpgradeCounts counts = _upgradeBoardService.GetUpgradeCounts(_gs.user, boardData);

            _uiService.SetText(Header, "Upgrades:   (" + counts.CurrUpgrades + "/" + counts.TotalUpgrades + ")");

        }
    }
}
