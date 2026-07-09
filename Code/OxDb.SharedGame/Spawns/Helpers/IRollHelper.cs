using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Spawns.Helpers
{
    public interface IRollHelper : ISetupDictionaryItem<long>
    {
        ValueTask<long> GetQuantityMult(IUnitDataLookup lookup, RollLootArgs rollLootArgs, long entityId);
        ValueTask<List<RewardList>> Roll<SI>(IUnitDataLookup lookup, SI spawnItem, long rewardSourceId, RollLootArgs rollLootArgs) where SI : ISpawnItem;
    }
}


