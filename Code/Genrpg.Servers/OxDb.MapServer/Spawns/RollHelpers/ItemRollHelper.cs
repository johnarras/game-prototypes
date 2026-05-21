using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.Services;
using OxDb.SharedGame.Inventory.Settings.ItemTypes;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Spawns.Settings;
using System.Collections.Generic;

namespace OxDb.MapServer.Spawns.RollHelpers
{
    public class ItemRollHelper : IRollHelper
    {
        public long HelperKey => EntityTypes.Item;

        private IItemGenService _itemGenService = null;
        private IGameData _gameData = null;
        private IRewardService _rewardService = null;

        public List<RewardList> Roll<SI>(IRandom rand, long rewardSourceId, RollLootArgs rollLootArgs, SI spawnItem) where SI : ISpawnItem
        {
            List<RewardList> retval = new List<RewardList>();

            ItemType itype = _gameData.Get<ItemTypeSettings>(null).Get(spawnItem.EntityId);

            if (itype == null)
            {
                return retval;
            }

            RewardList rewardList = _rewardService.CreateRewardList(rewardSourceId, new List<Reward>(), spawnItem.EntityId);
            retval.Add(rewardList);
            long quantity = RandUtils.LongRange(spawnItem.MinQuantity, spawnItem.MaxQuantity, rand);

            ItemGenArgs igd = new ItemGenArgs()
            {
                ItemTypeId = spawnItem.EntityId,
                Level = rollLootArgs.Level,
                QualityTypeId = rollLootArgs.QualityTypeId,
                Quantity = 1,
            };

            for (int i = 0; i < quantity; i++)
            {
                Reward rew = new Reward();
                rew.EntityId = spawnItem.EntityId;
                rew.EntityTypeId = EntityTypes.Item;
                rew.Quantity = 1;
                rewardList.Rewards.Add(rew);

                rew.ExtraData = _itemGenService.Generate(rand, igd);
            }
            return retval;
        }
    }
}


