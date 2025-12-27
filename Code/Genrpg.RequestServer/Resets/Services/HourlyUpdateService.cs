using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Services;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.UserEnergy.WebApi;

namespace Genrpg.RequestServer.Resets.Services
{
    public class HourlyUpdateService : IHourlyUpdateService
    {
        private IGameData _gameData = null;
        private ICoreCurrencyService _coreCurrencyService = null;
        private IWebRewardService _rewardService = null;
        public async Task CheckHourlyCurrencyUpdate(WebContext context, bool onLogin)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>();


            DateTime nowTime = DateTime.UtcNow;
            if (userData.NextHourlyUpdate > nowTime)
            {
                return;
            }

            int resetHours = (int)(nowTime - userData.NextHourlyUpdate).TotalHours + 1;


            IReadOnlyList<CoreCurrencyType> currencies = _gameData.Get<CoreCurrencyTypeSettings>(context.user).GetData();

            TraderStatData statData = await context.GetAsync<TraderStatData>();

            List<Reward> newRewards = new List<Reward>();

            foreach (CoreCurrencyType ctype in currencies)
            {
                long regenVal = _coreCurrencyService.GetRegen(ctype.IdKey, userData, statData);
                long storageVal = _coreCurrencyService.GetStorage(ctype.IdKey, userData, statData);

                long currVal = userData.Currencies.Get(ctype.IdKey);

                if (currVal >= storageVal)
                {
                    continue;
                }

                long maxAdded = storageVal - currVal;

                long newRegen = Math.Min(maxAdded, resetHours * regenVal);

                if (newRegen > 0)
                {
                    newRewards.Add(new Reward() { EntityTypeId = EntityTypes.CoreCurrency, EntityId = ctype.IdKey, Quantity = newRegen });
                }
            }


            await _rewardService.GiveRewardsAsync(context, newRewards, new RewardParams());
            context.user.SetNextHourlyUpdate();

            if (!onLogin)
            {
                context.AddResponse(new UpdateCoreCurrenciesResponse() { Rewards = newRewards, NextHourlyUpdate = context.user.NextHourlyUpdate });
            }
        }
    }
}


