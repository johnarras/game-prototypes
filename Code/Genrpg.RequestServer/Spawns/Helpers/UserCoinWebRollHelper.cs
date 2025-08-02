using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Upgrades.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.UserCoins.Constants;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.UserStats.Constants;

namespace Genrpg.RequestServer.Spawns.Helpers
{
    public class UserCoinWebRollHelper : BaseWebRollHelper
    {

        private IUpgradeBoardService _upgradeBoardServce = null;

        public override long Key => EntityTypes.UserCoin;

        public override async Task<long> GetQuantityMult(WebContext context, RollData rollData, long entityId)
        {
            return _upgradeBoardServce.GetCoinRewardMult(context.user, await context.GetAsync<BoardData>(), entityId);
        }
    }
}
