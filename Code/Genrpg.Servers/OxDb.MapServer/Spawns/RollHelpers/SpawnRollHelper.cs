using OxDb.MapServer.Spawns.Services;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Spawns.Settings;
using System.Collections.Generic;

namespace OxDb.MapServer.Spawns.RollHelpers
{
    public class SpawnRollHelper : IRollHelper
    {
        public long HelperKey => EntityTypes.Spawn;

        private ISpawnService _spawnService = null;
        private IGameData _gameData = null;
        public List<RewardList> Roll<SI>(IRandom rand, long rewardSourceId, RollLootArgs rollLootArgs, SI item) where SI : ISpawnItem
        {
            List<RewardList> retval = new List<RewardList>();
            long quantity = RandUtils.LongRange(item.MinQuantity, item.MaxQuantity, rand);

            SpawnTable st = _gameData.Get<SpawnSettings>(null).Get(item.EntityId);
            if (st != null)
            {
                for (int j = 0; j < quantity; j++)
                {
                    rollLootArgs.Depth++;
                    List<RewardList> list2 = _spawnService.Roll(rand, st.Items, rewardSourceId, rollLootArgs);
                    rollLootArgs.Depth--;
                    retval.AddRange(list2);
                }
            }
            return retval;
        }
    }
}


