using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Spawns.Interfaces
{
    public interface IRollHelper : ISetupDictionaryItem<long>
    {
        List<RewardList> Roll<SI>(IRandom rand, RollLootArgs rollLootArgs, SI item) where SI : ISpawnItem;
    }
}


