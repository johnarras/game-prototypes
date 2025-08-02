using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Upgrades.WebApi;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;

namespace Assets.Scripts.BoardGame.Services
{
    public interface IClientUpgradeBoardService : IInjectable
    {
        void OnReceiveUpgradeResponse(UpgradeBoardResponse response, CancellationToken token);
    }

    public class ClientUpgradeBoardService : IClientUpgradeBoardService
    {

        private IDispatcher _dispatcher;
        private IClientGameState _gs;
        private IGameData _gameData;
        private IRewardService _rewardService;
        public void OnReceiveUpgradeResponse(UpgradeBoardResponse response, CancellationToken token)
        {
            bool hadError = false;
            bool showedError = false;
            if (!response.Success)
            {
                hadError = true;
                if (!string.IsNullOrEmpty(response.Message))
                {
                    _dispatcher.Dispatch(new ShowFloatingText(response.Message, EFloatingTextArt.Error));
                    showedError = true;
                }
            }

            if (response.Costs != null && !response.Costs.CanUpgradeNow && !string.IsNullOrEmpty(response.Costs.ErrorMessage))
            {
                hadError = true;
                _dispatcher.Dispatch(new ShowFloatingText(response.Costs.ErrorMessage, EFloatingTextArt.Error));
                showedError = true;
            }

            if (hadError)
            {
                if (!showedError)
                {
                    _dispatcher.Dispatch(new ShowFloatingText("Error upgrading tile, please refresh.", EFloatingTextArt.Error));
                }


                return;
            }


            BoardData boardData = _gs.ch.Get<BoardData>();

            boardData.TileTypeUpgradeTiers.Set(response.TileTypeId, (short)response.NewTier);


            foreach (UpgradeReagent reagent in response.Costs.Reagents)
            {
                _rewardService.Add(_gs.ch, EntityTypes.UserCoin, reagent.UserCoinTypeId, -reagent.RequiredQuantity, null);
            }

            _dispatcher.Dispatch(response);

        }
    }
}
