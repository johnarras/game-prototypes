using Assets.Scripts.Entities.UI;
using Assets.Scripts.UI.SmallUIPieces;
using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Upgrades.Services;
using Genrpg.Shared.BoardGame.Upgrades.WebApi;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Tiles.Settings;
using Genrpg.Shared.Users.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Tiles.UI
{

    public class UpgradeStates
    {
        public const int NoTier = 0;
        public const int NotUpgraded = 1;
        public const int Upgraded = 2;

    }

    public class TileUpgradeUI : BaseBehaviour
    {

        public GText NameText;
        public GButton UpgradeButton;
        public GButton RepairButton;

        public GameObject UpgradeCostsAnchor;
        public EntityIcon UpgradeIcon;

        public List<MultiStateIcon> UpgradeTierIcons = new List<MultiStateIcon>();

        protected IUpgradeBoardService _upgradeBoardService;
        protected IClientWebService _webService;

        private TileType _tileType;


        public override void Init()
        {
            base.Init();
            _dispatcher.AddListener<UpgradeBoardResponse>(OnUpgradeBoard, GetToken());
        }


        public void SetData(long tileTypeId)
        {
            _tileType = _gameData.Get<TileTypeSettings>(_gs.user).Get(tileTypeId);
            _clientEntityService.DestroyAllChildren(UpgradeCostsAnchor);

            if (_tileType == null || _tileType.UpgradeReagents.Count < 1)
            {
                _clientEntityService.SetActive(gameObject, false);
                return;
            }

            _uiService.SetText(NameText, _tileType.Name);
            _uiService.SetButton(UpgradeButton, GetType().Name, UpgradeTile);
            _uiService.SetButton(RepairButton, GetType().Name, RepairTile);
            _clientEntityService.SetActive(gameObject, true);

            CoreUserData userData = _gs.ch.Get<CoreUserData>();
            BoardData boardData = _gs.ch.Get<BoardData>();

            long startTier = _upgradeBoardService.GetStartUpgradeTier(_gs.user, _gs.user.Level);

            long endTier = _upgradeBoardService.GetEndUpgradeTier(_gs.user, _gs.user.Level);

            long currTier = boardData.GetCurrentUpgradeTier(tileTypeId);

            long totalTiers = endTier - startTier;

            for (int i = 0; i < UpgradeTierIcons.Count; i++)
            {
                if (i >= totalTiers)
                {
                    UpgradeTierIcons[i].SetState(UpgradeStates.NoTier);
                }
                else if (i < currTier)
                {
                    UpgradeTierIcons[i].SetState(UpgradeStates.Upgraded);
                }
                else
                {
                    UpgradeTierIcons[i].SetState(UpgradeStates.NotUpgraded);

                }
            }
            UpgradeCosts costs = _upgradeBoardService.GetUpgradeCosts(_gs.user, userData, tileTypeId, currTier);

            _clientEntityService.SetActive(RepairButton, false);
            if (!costs.CanUpgradeNow)
            {
                _clientEntityService.SetActive(UpgradeButton, false);
            }

            foreach (UpgradeReagent reagent in costs.Reagents)
            {
                EntityIcon icon = _clientEntityService.FullInstantiate(UpgradeIcon);
                _clientEntityService.AddToParent(icon, UpgradeCostsAnchor);
                icon.SetData(EntityTypes.UserCoin, reagent.UserCoinTypeId, reagent.CurrQuantity, reagent.RequiredQuantity);
            }
        }

        private async Task UpgradeTile(CancellationToken token)
        {
            _logService.Info("UpgradeTile 1: " + _tileType.Name);
            await _webService.SendClientUserWebRequestAsync<UpgradeBoardResponse>(new UpgradeBoardRequest() { TileTypeId = _tileType.IdKey }, GetToken());
            _logService.Info("UpgradeTile 2: " + _tileType.Name);
        }

        private void RepairTile()
        {
            _logService.Info("RepairTile: " + _tileType.Name);
        }
        private void OnUpgradeBoard(UpgradeBoardResponse response)
        {
            SetData(_tileType.IdKey);
        }
    }
}
