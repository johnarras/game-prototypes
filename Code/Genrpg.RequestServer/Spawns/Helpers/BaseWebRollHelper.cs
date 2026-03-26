using Genrpg.RequestServer.Core;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;

namespace Genrpg.RequestServer.Spawns.Helpers
{
    public abstract class BaseWebRollHelper : IWebRollHelper
    {
        public abstract long HelperKey { get; }
        public virtual async Task<List<Reward>> Roll<SI>(WebContext context, RollLootArgs rollLootArgs, SI si) where SI : ISpawnItem
        {
            long mult = await GetQuantityMult(context, rollLootArgs, si.EntityId);

            long quantity = RandUtils.LongRange(si.MinQuantity * mult, si.MaxQuantity * mult, context.rand);

            List<Reward> retval = new List<Reward>();

            Reward rew = new Reward();
            rew.EntityId = si.EntityId;
            rew.EntityTypeId = si.EntityTypeId;
            rew.Quantity = quantity;
            retval.Add(rew);

            return retval;
        }

        public virtual async Task<long> GetQuantityMult(WebContext context, RollLootArgs rollLootArgs, long entityId)
        {
            await Task.CompletedTask;
            return 1;
        }
    }
}


