using Genrpg.MapServer.Spawns.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.MapServer.Spawns.RollHelpers
{
    public class SpawnRollHelper : IRollHelper
    {
        public long HelperKey => EntityTypes.Spawn;

        private ISpawnService _spawnService = null;
        private IGameData _gameData = null;
        public List<RewardList> Roll<SI>(IRandom rand, RollLootArgs rollLootArgs, SI item) where SI : ISpawnItem
        {
            List<RewardList> retval = new List<RewardList>();
            long quantity = MathUtils.LongRange(item.MinQuantity, item.MaxQuantity, rand);

            SpawnTable st = _gameData.Get<SpawnSettings>(null).Get(item.EntityId);
            if (st != null)
            {
                for (int j = 0; j < quantity; j++)
                {
                    rollLootArgs.Depth++;
                    List<RewardList> list2 = _spawnService.Roll(rand, st.Items, rollLootArgs);
                    rollLootArgs.Depth--;
                    retval.AddRange(list2);
                }
            }
            return retval;
        }
    }
}
