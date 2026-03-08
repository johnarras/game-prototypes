using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Services;
using Genrpg.RequestServer.Spawns.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Cultures.Settings;
using Genrpg.Shared.Trader.Encounters.Entities;
using Genrpg.Shared.Trader.Travel.Entities;
using Genrpg.Shared.Utils;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ZstdSharp.Unsafe;

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
        private IWebRewardService _rewardService = null;

        public async Task TryCampingEncounter(WebContext context)
        {
            await Task.CompletedTask;
        }

        public async Task<EncounterResult> TryEndOfTravelDayEncounter(WebContext context, TravelStatus status, TravelDay day)
        {


            TravelEncounterSettings encounterSettings = _gameData.Get<TravelEncounterSettings>(context.core);

            double goodChance = encounterSettings.GoodEncounterChance + context.core.Vars[TraderVars.GoodEventChance] / 100.0;

            double badChance = encounterSettings.BadEncounterChance + context.core.Vars[TraderVars.BadEventChance] / 100.0;


            TravelEncounter chosenEncounter = null;

            

            if (context.rand.NextDouble() < goodChance)
            {
                chosenEncounter = RandomUtils.GetRandomElement(encounterSettings.GetGoodEncounters(), context.rand);
            }

            if (chosenEncounter == null && context.rand.NextDouble() < badChance)
            {
                chosenEncounter = RandomUtils.GetRandomElement(encounterSettings.GetBadEncounters(), context.rand);
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

                        long currCurrencyQuantity = context.core.Currencies[rew.EntityId];

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

            RewardParams rp= new RewardParams();
            await _rewardService.GiveRewardsAsync(context, result.RewardLists, rp);

            result.Message = chosenEncounter.Text;

            return result;
        }
    }
}
