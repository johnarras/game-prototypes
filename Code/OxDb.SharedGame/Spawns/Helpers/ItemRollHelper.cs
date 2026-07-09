using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.Services;
using OxDb.SharedGame.Inventory.Settings.ItemTypes;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Spawns.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Spawns.Helpers
{
    public class ItemRollHelper : IRollHelper
    {
        public long HelperKey => EntityTypes.Item;

        private IItemGenService _itemGenService = null;
        private IGameData _gameData = null;
        private IRewardService _rewardService = null;

        public async ValueTask<List<RewardList>> Roll<SI>(IUnitDataLookup lookup, SI spawnItem, long rewardSourceId, RollLootArgs rollLootArgs) where SI : ISpawnItem
        {
            ItemType itype = _gameData.Get<ItemTypeSettings>(await lookup.GetFilteredObject()).Get(spawnItem.EntityId);

            if (itype == null)
            {
                return new List<RewardList>();
            }

            long quantity = RandUtils.LongRange(spawnItem.MinQuantity, spawnItem.MaxQuantity, lookup.Rand);

            ItemGenArgs igd = new ItemGenArgs()
            {
                ItemTypeId = spawnItem.EntityId,
                Level = rollLootArgs.Level,
                QualityTypeId = rollLootArgs.QualityTypeId,
                Quantity = 1,
            };

            List<Reward> rewards = new List<Reward>();

            for (int i = 0; i < quantity; i++)
            {
                Reward rew = new Reward();
                rew.EntityId = spawnItem.EntityId;
                rew.EntityTypeId = EntityTypes.Item;
                rew.Quantity = 1;
                rewards.Add(rew);

                rew.ExtraData = _itemGenService.Generate(lookup.Rand, igd);
            }

            return _rewardService.CreateListFromList(rewardSourceId, spawnItem.EntityId, rewards);
        }

        public async ValueTask<long> GetQuantityMult(IUnitDataLookup lookup, RollLootArgs rollLootArgs, long entityId)
        {
            await Task.CompletedTask;
            return 1;
        }
    }
}


