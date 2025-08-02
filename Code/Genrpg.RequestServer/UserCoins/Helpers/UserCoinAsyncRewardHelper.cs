using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Interfaces;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Upgrades.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Settings.Qualities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.UserCoins.Constants;
using Genrpg.Shared.UserCoins.Settings;
using Genrpg.Shared.Users.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.UserCoins.Helpers
{
    public class UserCoinAsyncRewardHelper : IAsyncRewardHelper
    {
        private IUpgradeBoardService _upgradeBoardService = null;

        public long Key => EntityTypes.UserCoin;

        public async Task GiveRewardsAsync(WebContext context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            if (quantity > 0)
            {
                long coinCap = _upgradeBoardService.GetMaxCoinStorage(context.user, await context.GetAsync<BoardData>(), entityId);

                if (coinCap > 0)
                {
                    long currVal = userData.Coins.Get(entityId);

                    long newVal = currVal + quantity;

                    if (newVal > coinCap)
                    {
                        newVal = coinCap - currVal;
                        quantity = newVal;
                    }
                }
            }

            userData.Coins.Add(entityId, quantity);
        }
    }
}
