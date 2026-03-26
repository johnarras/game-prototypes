using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Spawns.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Encounters.Entities;
using Genrpg.Shared.Trader.Encounters.Settings;
using Genrpg.Shared.Trader.Travel.Entities;
using Genrpg.Shared.Utils;

namespace Genrpg.RequestServer.Trader.Encounters.Services
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

            if (context.rand.NextDouble() < goodChance)
            {
                chosenEncounter = RandUtils.GetRandomElement(encounterSettings.GetGoodEncounters(), context.rand);
            }

            if (chosenEncounter == null && context.rand.NextDouble() < badChance)
            {
                chosenEncounter = RandUtils.GetRandomElement(encounterSettings.GetBadEncounters(), context.rand);
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

            List<RewardList> goodRewards = await _spawnService.Roll(context, chosenEncounter.GoodEffects, rollArgs);

            List<RewardList> badRewards = await _spawnService.Roll(context, chosenEncounter.BadEffects, rollArgs);

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
                result.RewardLists.AddRange(await _spawnService.Roll(context, chosenEncounter.FailureEffects, rollArgs));
            }

            RewardParams rp = new RewardParams();
            await _rewardService.GiveRewards(context, result.RewardLists, rp);

            result.Message = chosenEncounter.Text;

            return result;
        }
    }
}
