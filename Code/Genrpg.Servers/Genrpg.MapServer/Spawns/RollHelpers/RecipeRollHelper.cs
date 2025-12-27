using Genrpg.MapServer.Crafting.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.MapServer.Spawns.RollHelpers
{
    public class RecipeRollHelper : IRollHelper
    {
        public long HelperKey => EntityTypes.Recipe;

        private IServerCraftingService _craftingService = null;
        public List<RewardList> Roll<SI>(IRandom rand, RollLootArgs rollLootArgs, SI spawnItem) where SI : ISpawnItem
        {
            List<RewardList> retval = new List<RewardList>();
            RewardList rewardList = new RewardList();
            Item newItem = _craftingService.GenerateRecipeReward(rand, rollLootArgs.Level);
            if (newItem != null)
            {
                Reward rew = new Reward();
                rew.EntityId = newItem.ItemTypeId;
                rew.EntityTypeId = EntityTypes.Item;
                rew.Quantity = 1;
                rew.QualityTypeId = rollLootArgs.QualityTypeId;
                rew.Level = rollLootArgs.Level;
                rewardList.Rewards.Add(rew);
            }
            return retval;
        }
    }
}


