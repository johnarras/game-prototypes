using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Spawns.Helpers
{
    public abstract class BaseRollHelper : IRollHelper
    {

        protected IRewardService _rewardService = null;
        public abstract long HelperKey { get; }
        public virtual async ValueTask<List<RewardList>> Roll<SI>(IUnitDataLookup lookup, SI si, long rewardSourceId, RollLootArgs rollLootArgs) where SI : ISpawnItem
        {
            long mult = await GetQuantityMult(lookup, rollLootArgs, si.EntityId);

            long quantity = RandUtils.LongRange(si.MinQuantity * mult, si.MaxQuantity * mult, lookup.Rand);

            Reward rew = new Reward();
            rew.EntityId = si.EntityId;
            rew.EntityTypeId = si.EntityTypeId;
            rew.Quantity = quantity;

            return _rewardService.CreateListFromReward(rewardSourceId, si.EntityId, rew);
        }

        public virtual async ValueTask<long> GetQuantityMult(IUnitDataLookup lookup, RollLootArgs rollLootArgs, long entityId)
        {
            await Task.CompletedTask;
            return 1;
        }
    }
}


