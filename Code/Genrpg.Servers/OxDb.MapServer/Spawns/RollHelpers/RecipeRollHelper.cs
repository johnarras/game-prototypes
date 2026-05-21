using OxDb.MapServer.Crafting.Services;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Spawns.Settings;
using System.Collections.Generic;

namespace OxDb.MapServer.Spawns.RollHelpers
{
    public class RecipeRollHelper : IRollHelper
    {
        public long HelperKey => EntityTypes.Recipe;

        private IServerCraftingService _craftingService = null;
        private IRewardService _rewardService = null;
        public List<RewardList> Roll<SI>(IRandom rand, long rewardSourceId, RollLootArgs rollLootArgs, SI spawnItem) where SI : ISpawnItem
        {
            List<RewardList> retval = new List<RewardList>();
            RewardList rewardList = _rewardService.CreateRewardList(rewardSourceId, new List<Reward>(), spawnItem.EntityId);
            Item newItem = _craftingService.GenerateRecipeReward(rand, rollLootArgs.Level);
            if (newItem != null)
            {
                Reward rew = new Reward();
                rew.EntityId = newItem.ItemTypeId;
                rew.EntityTypeId = EntityTypes.Item;
                rew.Quantity = 1;
                rewardList.Rewards.Add(rew);
            }
            return retval;
        }
    }
}


