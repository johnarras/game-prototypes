using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Helpers;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Spawns.Settings;
using OxDb.SharedGame.Units.Loaders;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Spawns.Services
{

    public interface ISpawnService : IInitializable
    {
        IRollHelper GetRollHelper(long entityTypeid);
        ValueTask<List<RewardList>> Roll(IUnitDataLookup lookup, long spawnTableId, long rewardSourceId, RollLootArgs rollLootArgs);
        ValueTask<List<RewardList>> Roll(IUnitDataLookup lookup, SpawnTable st, long rewardSourceId, RollLootArgs rollLootArgs);
        ValueTask<List<RewardList>> Roll<SI>(IUnitDataLookup lookup, List<SI> items, long rewardSourceId, RollLootArgs rollLootArgs) where SI : ISpawnItem;
    }

    /// <summary>
    /// This class is used to roll treasure and other items. 
    /// It's set up so that it's possible to say have a generic
    /// humanoid monster SpawnTable that gives level appropriate loot,
    /// and then for specific monsters to create a new parent spawn table
    /// that always rolls this generic table once, and then adds some extra 
    /// loot. 
    /// </summary>
    public class SpawnService : ISpawnService
    {
        private IGameData _gameData = null;
        private IRewardService _rewardService = null;
        private SetupDictionaryContainer<long, IRollHelper> _rollHelpers = new();
        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public IRollHelper GetRollHelper(long entityTypeid)
        {
            if (_rollHelpers.TryGetValue(entityTypeid, out IRollHelper helper))
            {
                return helper;
            }
            return null;
        }

        public async ValueTask<List<RewardList>> Roll(IUnitDataLookup lookup, long spawnTableId, long rewardSourceId, RollLootArgs rollLootArgs)
        {
            SpawnTable table = null;
            if (lookup is IFilteredObject filtered)
            {
                table = _gameData.Get<SpawnSettings>(filtered).Get(spawnTableId);
            }
            else
            {
                CoreData coreData = await lookup.GetAsync<CoreData>();
                table = _gameData.Get<SpawnSettings>(coreData).Get(spawnTableId);
            }

            return await Roll(lookup, table, rewardSourceId, rollLootArgs);
        }

        // Different public roll methods.

        public async ValueTask<List<RewardList>> Roll(IUnitDataLookup lookup, SpawnTable st, long rewardSourceId, RollLootArgs rollLootArgs)
        {
            if (st == null)
            {
                return new List<RewardList>();
            }

            return await Roll(lookup, st.Items, rewardSourceId, rollLootArgs);
        }

        public async ValueTask<List<RewardList>> Roll<SI>(IUnitDataLookup lookup, List<SI> items, long rewardSourceId, RollLootArgs rollLootArgs) where SI : ISpawnItem
        {
            return await InnerRoll(lookup, items, rewardSourceId, rollLootArgs);
        }

        private async ValueTask<List<RewardList>> InnerRoll<SI>(IUnitDataLookup lookup, List<SI> items, long rewardSourceId, RollLootArgs rollLootArgs) where SI : ISpawnItem
        {
            List<RewardList> retval = _rewardService.CreateListFromReward(rewardSourceId, rollLootArgs.EntityId);

            for (int i = 0; i < rollLootArgs.Times; i++)
            {
                rollLootArgs.Depth++;
                retval.AddRange(await RollOnce(lookup, items, rewardSourceId, rollLootArgs));
                rollLootArgs.Depth--;
            }

            return retval;
        }


        /// <summary>
        /// Roll against the spawn table once.
        /// </summary>
        /// <param name="gs">GameState</param>
        /// <param name="st">Spawn Table to roll on</param>
        /// <param name="level">Level of loot</param>
        /// <param name="qualityTypeId">Power of the loot</param>
        /// <param name="depth">Depth of the recursion</param>
        /// <returns>A retval of spawn responses</returns>
        private async ValueTask<List<RewardList>> RollOnce<SI>(IUnitDataLookup lookup, List<SI> items, long rewardSourceId, RollLootArgs rollLootArgs) where SI : ISpawnItem
        {
            if (items == null)
            {
                return new List<RewardList>();
            }

            List<RewardList> retval = new List<RewardList>();

            Dictionary<int, List<SI>> groupDict = new Dictionary<int, List<SI>>();
            List<SI> rollEachList = new List<SI>();
            foreach (SI si in items)
            {
                if (si.MinLevel > rollLootArgs.Level)
                {
                    continue;
                }
                if (si.GroupId < 1)
                {
                    if (lookup.Rand.NextDouble() * 100 < si.Weight)
                    {
                        rollLootArgs.Depth++;
                        retval = retval.Concat(await RollOneItem(lookup, si, rewardSourceId, rollLootArgs)).ToList();
                        rollLootArgs.Depth--;
                    }
                    continue;
                }
                if (!groupDict.ContainsKey(si.GroupId))
                {
                    groupDict[si.GroupId] = new List<SI>();
                }
                groupDict[si.GroupId].Add(si);

            }

            foreach (int key in groupDict.Keys)
            {
                SI si = RandUtils.GetRandomElement(groupDict[key], lookup.Rand);

                if (si != null)
                {
                    rollLootArgs.Depth++;
                    retval = retval.Concat(await RollOneItem(lookup, si, rewardSourceId, rollLootArgs)).ToList();
                    rollLootArgs.Depth--;
                }
            }
            return retval;
        }


        private async ValueTask<List<RewardList>> RollOneItem<SI>(IUnitDataLookup lookup, SI si, long rewardSourceId, RollLootArgs rollLootArgs) where SI : ISpawnItem
        {
            List<RewardList> retval = new List<RewardList>();

            if (rollLootArgs.Depth > 10)
            {
                return retval;
            }

            IRollHelper rollHelper = GetRollHelper(si.EntityTypeId);

            if (rollHelper != null)
            {
                retval = await rollHelper.Roll(lookup, si, rewardSourceId, rollLootArgs);
                return retval;
            }

            long quantity = RandUtils.LongRange(si.MinQuantity, si.MaxQuantity, lookup.Rand);

            Reward rew = new Reward();
            rew.EntityId = si.EntityId;
            rew.EntityTypeId = si.EntityTypeId;
            rew.Quantity = quantity;
            List<RewardList> rlist = _rewardService.CreateListFromReward(rewardSourceId, rew.EntityId, rew);

            retval.AddRange(rlist);

            return retval;
        }


    }
}


