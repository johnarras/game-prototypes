using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Spawns.Services;
using OxDb.SharedGame.Spawns.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Spawns.Helpers
{
    public class SpawnRollHelper : IRollHelper
    {
        public long HelperKey => EntityTypes.Spawn;

        private ISpawnService _spawnService = null;
        private IGameData _gameData = null;
       
        public async ValueTask<long> GetQuantityMult(IUnitDataLookup lookup, RollLootArgs rollLootArgs, long entityId)
        {
            return 1;
        }

        public async ValueTask<List<RewardList>> Roll<SI>(IUnitDataLookup lookup, SI spawnIte, long rewardSourceId, RollLootArgs rollLootArgs) where SI : ISpawnItem
        {

            List<RewardList> rewards = new List<RewardList>();
            long quantity = RandUtils.LongRange(spawnIte.MinQuantity, spawnIte.MaxQuantity, lookup.Rand);

            SpawnTable st = _gameData.Get<SpawnSettings>(await lookup.GetFilteredObject()).Get(spawnIte.EntityId);
            if (st != null)
            {
                for (int j = 0; j < quantity; j++)
                {
                    rollLootArgs.Depth++;
                    List<RewardList> list2 = await _spawnService.Roll(lookup, st.Items, rewardSourceId, rollLootArgs);
                    rollLootArgs.Depth--;
                    rewards.AddRange(list2);
                }
            }

            return rewards;
        }
    }
}


