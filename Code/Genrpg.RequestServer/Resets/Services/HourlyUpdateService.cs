using Genrpg.RequestServer.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Services;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.UserEnergy.WebApi;

namespace Genrpg.RequestServer.Resets.Services
{
    public class HourlyUpdateService : IHourlyUpdateService
    {
        private IGameData _gameData = null;
        private ICoreCurrencyService _coreCurrencyService = null;
        public async Task CheckHourlyCurrencyUpdate(WebContext context)
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
                    userData.Currencies.Add(ctype.IdKey, newRegen);
                    newRewards.Add(new Reward() { EntityTypeId = EntityTypes.CoreCurrency, EntityId = ctype.IdKey, Quantity = newRegen });
                }
            }

            context.user.SetNextHourlyUpdate();

            context.Responses.AddResponse(new UpdateCoreCurrenciesResponse() { Rewards = newRewards, NextHourlyUpdate = context.user.NextHourlyUpdate });
        }
    }
}
