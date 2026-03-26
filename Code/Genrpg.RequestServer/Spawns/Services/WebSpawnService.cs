using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Spawns.Helpers;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;

namespace Genrpg.RequestServer.Spawns.Services
{

    public interface IWebSpawnService : IInitializable
    {
        IWebRollHelper GetRollHelper(long entityTypeid);
        Task<List<RewardList>> Roll(WebContext context, long spawnTableId, RollLootArgs rollLootArgs);
        Task<List<RewardList>> Roll(WebContext context, SpawnTable st, RollLootArgs rollLootArgs);
        Task<List<RewardList>> Roll<SI>(WebContext context, List<SI> items, RollLootArgs rollLootArgs) where SI : ISpawnItem;
    }

    /// <summary>
    /// This class is used to roll treasure and other items. 
    /// It's set up so that it's possible to say have a generic
    /// humanoid monster SpawnTable that gives level appropriate loot,
    /// and then for specific monsters to create a new parent spawn table
    /// that always rolls this generic table once, and then adds some extra 
    /// loot. 
    /// </summary>
    public class WebSpawnService : IWebSpawnService
    {
        private IGameData _gameData = null;
        private SetupDictionaryContainer<long, IWebRollHelper> _rollHelpers = new();
        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public IWebRollHelper GetRollHelper(long entityTypeid)
        {
            if (_rollHelpers.TryGetValue(entityTypeid, out IWebRollHelper helper))
            {
                return helper;
            }
            return null;
        }

        public async Task<List<RewardList>> Roll(WebContext context, long spawnTableId, RollLootArgs rollLootArgs)
        {
            return await Roll(context, _gameData.Get<SpawnSettings>(null).Get(spawnTableId), rollLootArgs);
        }

        // Different public roll methods.

        public async Task<List<RewardList>> Roll(WebContext context, SpawnTable st, RollLootArgs rollLootArgs)
        {
            if (st == null)
            {
                return new List<RewardList>();
            }

            return await Roll(context, st.Items, rollLootArgs);
        }

        public async Task<List<RewardList>> Roll<SI>(WebContext context, List<SI> items, RollLootArgs rollLootArgs) where SI : ISpawnItem
        {
            return await InnerRoll(context, items, rollLootArgs);
        }

        private async Task<List<RewardList>> InnerRoll<SI>(WebContext context, List<SI> items, RollLootArgs rollLootArgs) where SI : ISpawnItem
        {
            List<RewardList> list = new List<RewardList>();

            list.Add(new RewardList() { RewardSourceId = rollLootArgs.RewardSourceId, EntityId = rollLootArgs.EntityId });
            for (int i = 0; i < rollLootArgs.Times; i++)
            {
                rollLootArgs.Depth++;
                list[0].Rewards = list[0].Rewards.Concat(await RollOnce(context, items, rollLootArgs)).ToList();
                rollLootArgs.Depth--;
            }

            return list;
        }


        /// <summary>
        /// Roll against the spawn table once.
        /// </summary>
        /// <param name="gs">GameState</param>
        /// <param name="st">Spawn Table to roll on</param>
        /// <param name="level">Level of loot</param>
        /// <param name="qualityTypeId">Power of the loot</param>
        /// <param name="depth">Depth of the recursion</param>
        /// <returns>A list of spawn responses</returns>
        private async Task<List<Reward>> RollOnce<SI>(WebContext context, List<SI> items, RollLootArgs rollLootArgs) where SI : ISpawnItem
        {
            if (items == null)
            {
                return new List<Reward>();
            }

            List<Reward> retval = new List<Reward>();

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
                    if (context.rand.NextDouble() * 100 < si.Weight)
                    {
                        rollLootArgs.Depth++;
                        retval = retval.Concat(await RollOneItem(context, si, rollLootArgs)).ToList();
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

                SI si = RandUtils.GetRandomElement(groupDict[key], context.rand);

                if (si != null)
                {
                    rollLootArgs.Depth++;
                    retval = retval.Concat(await RollOneItem(context, si, rollLootArgs)).ToList();
                    rollLootArgs.Depth--;
                }
            }
            return retval;
        }


        private async Task<List<Reward>> RollOneItem<SI>(WebContext context, SI si, RollLootArgs rollLootArgs) where SI : ISpawnItem
        {
            List<Reward> retval = new List<Reward>();

            if (rollLootArgs.Depth > 10)
            {
                return retval;
            }

            IWebRollHelper rollHelper = GetRollHelper(si.EntityTypeId);

            if (rollHelper != null)
            {
                retval = await rollHelper.Roll(context, rollLootArgs, si);
                return retval;
            }

            long quantity = RandUtils.LongRange(si.MinQuantity, si.MaxQuantity, context.rand);

            Reward rew = new Reward();
            rew.EntityId = si.EntityId;
            rew.EntityTypeId = si.EntityTypeId;
            rew.Quantity = quantity;
            retval.Add(rew);

            return retval;
        }


    }
}


