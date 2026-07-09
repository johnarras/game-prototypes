using OxDb.RequestServer.Core;
using OxDb.RequestServer.Resets.Entities;
using OxDb.RequestServer.Trader.Encounters.Services;
using OxDb.RequestServer.Trader.Stats.Services;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Attributes.PlayerData;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Services;
using OxDb.SharedGame.Currencies.Settings;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Trader.Camping.Settings.OxDb.SharedGame.Trader.Camping.Settings;
using OxDb.SharedGame.Trader.Constants;
using OxDb.SharedGame.UserEnergy.WebApi;

namespace OxDb.RequestServer.Resets.Services
{
    public interface IHourlyUpdateService : IInjectable
    {
        ValueTask CheckHourlyCurrencyUpdates(WebContext context, HourlyResetArgs args);
    }
    public class HourlyUpdateService : IHourlyUpdateService
    {
        private IGameData _gameData = null;
        private ICoreCurrencyService _coreCurrencyService = null;
        private IRewardService _rewardService = null;
        private ITravelEncounterService _encounterService = null;
        private IServerGameStatService _gameStatService = null;
        public async ValueTask CheckHourlyCurrencyUpdates(WebContext context, HourlyResetArgs args)
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

            AttributesData attributeData = await context.GetAsync<AttributesData>();

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


            await _rewardService.GiveRewards(context, newRewards, RewardSources.HourlyUpdate, new RewardParams());

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


