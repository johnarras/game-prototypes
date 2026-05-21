using OxDb.RequestServer.Core;
using OxDb.RequestServer.Spawns.Services;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Trader.Constants;
using OxDb.SharedGame.Trader.Encounters.Entities;
using OxDb.SharedGame.Trader.Encounters.Settings;
using OxDb.SharedGame.Trader.Travel.Entities;

namespace OxDb.RequestServer.Trader.Encounters.Services
{
    public interface ITravelEncounterService : IInjectable
    {
        Task<EncounterResult> TryEndOfTravelDayEncounter(WebContext context, TravelStatus status, TravelDay day);

        Task TryCampingEncounter(WebContext context);
    }

    public class TravelEncounterService : ITravelEncounterService
    {

        private IGameData _gameData = null;
        private IWebSpawnService _spawnService = null;
        private IRewardService _rewardService = null;

        public async Task TryCampingEncounter(WebContext context)
        {
            await Task.CompletedTask;
        }

        public async Task<EncounterResult> TryEndOfTravelDayEncounter(WebContext context, TravelStatus status, TravelDay day)
        {

            CoreData coreData = await context.GetAsync<CoreData>();
            TravelEncounterSettings encounterSettings = _gameData.Get<TravelEncounterSettings>(coreData);

            double goodChance = encounterSettings.GoodEncounterChance + coreData.Vars[TraderVars.Luck] / 100.0;

            double badChance = encounterSettings.BadEncounterChance - coreData.Vars[TraderVars.Luck] / 100.0;


            TravelEncounter chosenEncounter = null;

            if (context.Rand.NextDouble() < goodChance)
            {
                chosenEncounter = RandUtils.GetRandomElement(encounterSettings.GetGoodEncounters(), context.Rand);
            }

            if (chosenEncounter == null && context.Rand.NextDouble() < badChance)
            {
                chosenEncounter = RandUtils.GetRandomElement(encounterSettings.GetBadEncounters(), context.Rand);
            }

            if (chosenEncounter == null)
            {
                return null;
            }

            EncounterResult result = new EncounterResult();

            if (chosenEncounter.BadEffects.Any())
            {
                result.IsBad = true;
            }

            RollLootArgs rollArgs = new RollLootArgs()
            {

            };

            List<RewardList> goodRewards = await _spawnService.Roll(context, chosenEncounter.GoodEffects, RewardSources.TravelEncounter, rollArgs);

            List<RewardList> badRewards = await _spawnService.Roll(context, chosenEncounter.BadEffects, RewardSources.TravelEncounter, rollArgs);

            bool failedEncounter = false;

            foreach (RewardList rlist in badRewards)
            {
                foreach (Reward rew in rlist.Rewards)
                {
                    if (rew.EntityTypeId == EntityTypes.CoreCurrency)
                    {
                        if (rew.Quantity > 0)
                        {
                            rew.Quantity = -rew.Quantity;
                        }

                        long currCurrencyQuantity = coreData.Currencies[rew.EntityId];

                        if (Math.Abs(rew.Quantity) > currCurrencyQuantity)
                        {
                            rew.Quantity = -currCurrencyQuantity;
                            failedEncounter = true;
                        }
                    }
                }
            }

            result.RewardLists.AddRange(goodRewards);
            result.RewardLists.AddRange(badRewards);

            if (failedEncounter)
            {
                result.RewardLists.AddRange(await _spawnService.Roll(context, chosenEncounter.FailureEffects, RewardSources.TravelEncounter, rollArgs));
            }

            RewardParams rp = new RewardParams();
            await _rewardService.GiveRewards(context, result.RewardLists, rp);

            result.Message = chosenEncounter.Text;

            return result;
        }
    }
}
