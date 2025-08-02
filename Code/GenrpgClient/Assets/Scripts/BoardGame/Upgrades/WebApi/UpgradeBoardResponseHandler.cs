using Assets.Scripts.BoardGame.Services;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.BoardGame.Upgrades.WebApi;
using Genrpg.Shared.UserCoins.Constants;
using Genrpg.Shared.UserEnergy.WebApi;
using Genrpg.Shared.Users.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.BoardGame.Tiles.WebApi
{
    public class UgradeBoardResponseHandler : BaseClientWebResponseHandler<UpgradeBoardResponse>
    {
        private IClientUpgradeBoardService _clientUpgradeService = null!;
        protected override void InnerProcess(UpgradeBoardResponse result, CancellationToken token)
        {
            _clientUpgradeService.OnReceiveUpgradeResponse(result, token);
        }
    }
}
