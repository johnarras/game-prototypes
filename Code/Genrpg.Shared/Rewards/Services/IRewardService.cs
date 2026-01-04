using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Rewards.Services
{
    public interface IRewardService : IInjectable
    {
        bool GiveRewards<RL>(IRandom rand, MapObject obj, List<RL> resultList, RewardParams rp) where RL : RewardList;
        bool GiveReward(IRandom rand, MapObject obj, IReward res, RewardParams rp);
        bool GiveReward(IRandom rand, MapObject obj, long entityType, long entityId, long quantity, object extraData, RewardParams rp);
        bool Add(MapObject obj, long entityTypeId, long entityId, long quantity, RewardParams rp);
        bool Set(MapObject obj, long entityTypeId, long entityId, long quantity, RewardParams rp);
        void OnAddQuantity<TUpd>(MapObject obj, TUpd upd, long entityTypeId, long entityId, long diff, RewardParams rp) where TUpd : class, ISearchableItem;
    }
}


