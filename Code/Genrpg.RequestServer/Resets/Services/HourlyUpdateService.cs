using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Entities;
using Genrpg.RequestServer.Trader.Encounters.Services;
using Genrpg.RequestServer.Trader.Stats.Services;
using Genrpg.Shared.Attributes.PlayerData;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Trader.Camping.Settings.Genrpg.Shared.Trader.Camping.Settings;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.UserEnergy.WebApi;

namespace Genrpg.RequestServer.Resets.Services
{
    public class HourlyUpdateService : IHourlyUpdateService
    {
        private IGameData _gameData = null;
        private ICoreCurrencyService _coreCurrencyService = null;
        private IRewardService _rewardService = null;
        private ITravelEncounterService _encounterService = null;
        private IServerGameStatService _gameStatService = null;
        public async Task CheckHourlyCurrencyUpdate(WebContext context, HourlyResetArgs args)
        {
            CoreData coreData = await context.GetAsync<CoreData>();

            long regenHours = 0;
            if (!args.IsCamping)
            {
                DateTime nowTime = DateTime.UtcNow;
                if (coreData.NextHourlyUpdate > nowTime)
                {
                    return;
                }

                regenHours = (int)(nowTime - coreData.NextHourlyUpdate).TotalHours + 1;
            }
            else
            {
                CampingSettings campingSettings = _gameData.Get<CampingSettings>(coreData);

                regenHours = (args.InCity ? campingSettings.CityRegenHours : campingSettings.RoadRegenHours);
            }

            IReadOnlyList<CoreCurrencyType> currencies = _gameData.Get<CoreCurrencyTypeSettings>(coreData).GetData();

            AttributeData attributeData = await context.GetAsync<AttributeData>();

            List<Reward> newRewards = new List<Reward>();

            foreach (CoreCurrencyType ctype in currencies)
            {
                long regenVal = await _coreCurrencyService.GetRegen(context, ctype.IdKey);
                long storageVal = await _coreCurrencyService.GetStorage(context, ctype.IdKey);

                long currVal = coreData.Currencies[ctype.IdKey];

                if (currVal >= storageVal)
                {
                    continue;
                }

                long maxAdded = storageVal - currVal;

                long newRegen = Math.Min(maxAdded, regenHours * regenVal);

                if (newRegen > 0)
                {
                    newRewards.Add(new Reward() { EntityTypeId = EntityTypes.CoreCurrency, EntityId = ctype.IdKey, Quantity = newRegen });
                }
            }


            await _rewardService.GiveRewards(context, newRewards, new RewardParams());

            if (!args.IsCamping)
            {
                coreData.SetNextHourlyUpdate();
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
                    NextHourlyUpdate = coreData.NextHourlyUpdate,
                    Day = coreData.Vars[TraderVars.PlayCount],
                });
            }

            await _gameStatService.AddDebuffDaysPlayed(context, regenHours, true);

            if (args.IsCamping)
            {
                await _encounterService.TryCampingEncounter(context);
            }

        }
    }
}


