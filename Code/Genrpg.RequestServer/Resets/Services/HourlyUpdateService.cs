using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Entities;
using Genrpg.RequestServer.Rewards.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Services;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Camping.Settings.Genrpg.Shared.Trader.Camping.Settings;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.UserEnergy.WebApi;

namespace Genrpg.RequestServer.Resets.Services
{
    public class HourlyUpdateService : IHourlyUpdateService
    {
        private IGameData _gameData = null;
        private ICoreCurrencyService _coreCurrencyService = null;
        private IWebRewardService _rewardService = null;
        public async Task CheckHourlyCurrencyUpdate(WebContext context, HourlyResetArgs args)
        {
            CoreData coreData = await context.GetAsync<CoreData>();

            long resetHours = 0;
            if (!args.IsCamping)
            {
                DateTime nowTime = DateTime.UtcNow;
                if (coreData.NextHourlyUpdate > nowTime)
                {
                    return;
                }

                resetHours = (int)(nowTime - coreData.NextHourlyUpdate).TotalHours + 1;
            }
            else
            {
                CampingSettings campingSettings = _gameData.Get<CampingSettings>(context.core);

                resetHours = (args.InCity ? campingSettings.CityRegenHours : campingSettings.RoadRegenHours);
            }

            IReadOnlyList<CoreCurrencyType> currencies = _gameData.Get<CoreCurrencyTypeSettings>(context.core).GetData();

            TraderStatData statData = await context.GetAsync<TraderStatData>();

            List<Reward> newRewards = new List<Reward>();

            foreach (CoreCurrencyType ctype in currencies)
            {
                long regenVal = _coreCurrencyService.GetRegen(ctype.IdKey, coreData, statData);
                long storageVal = _coreCurrencyService.GetStorage(ctype.IdKey, coreData, statData);

                long currVal = coreData.Currencies[ctype.IdKey];

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

            if (!args.IsCamping)
            {
                context.core.SetNextHourlyUpdate();
            }
            else
            {
                coreData.Vars.Add(TraderVars.PlayCount, 1);
            }


            if (!args.OnLogin)
            {
                context.AddResponse(new HourlyUpdateResponse()
                {
                    Rewards = newRewards,
                    NextHourlyUpdate = context.core.NextHourlyUpdate,
                    Day = coreData.Vars[TraderVars.PlayCount],
                });
            }
        }
    }
}


