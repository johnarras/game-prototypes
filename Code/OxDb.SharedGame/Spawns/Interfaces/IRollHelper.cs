using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;

using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Settings;
using System.Collections.Generic;

namespace OxDb.SharedGame.Spawns.Interfaces
{
    public interface IRollHelper : ISetupDictionaryItem<long>
    {
        List<RewardList> Roll<SI>(IRandom rand, long rewardSourceId, RollLootArgs rollLootArgs, SI item) where SI : ISpawnItem;
    }
}


