using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Spawns.Helpers
{
    public class RecipeRollHelper : IRollHelper
    {
        public long HelperKey => EntityTypes.Recipe;

       // private IServerCraftingService _craftingService = null;
        private IRewardService _rewardService = null;
      
        public ValueTask<long> GetQuantityMult(IUnitDataLookup lookup, RollLootArgs rollLootArgs, long entityId)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<List<RewardList>> Roll<SI>(IUnitDataLookup lookup, SI spawnItem, long rewardSourceId, RollLootArgs rollLootArgs) where SI : ISpawnItem
        {
            Item newItem = null; // _craftingService.GenerateRecipeReward(rand, rollLootArgs.Level);
            if (newItem != null)
            {                
                Reward rew = new Reward();
                rew.EntityId = newItem.ItemTypeId;
                rew.EntityTypeId = EntityTypes.Item;
                rew.Quantity = 1;
                return _rewardService.CreateListFromReward(rewardSourceId, newItem.ItemTypeId, rew);
            }
            return new List<RewardList>();
        }
    }
}


