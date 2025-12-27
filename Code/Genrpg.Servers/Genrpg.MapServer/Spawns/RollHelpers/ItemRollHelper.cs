using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.MapServer.Spawns.RollHelpers
{
    public class ItemRollHelper : IRollHelper
    {
        public long HelperKey => EntityTypes.Item;

        private IItemGenService _itemGenService = null;
        private IGameData _gameData = null;

        public List<RewardList> Roll<SI>(IRandom rand, RollLootArgs rollLootArgs, SI spawnItem) where SI : ISpawnItem
        {
            List<RewardList> retval = new List<RewardList>();

            ItemType itype = _gameData.Get<ItemTypeSettings>(null).Get(spawnItem.EntityId);

            if (itype == null)
            {
                return retval;
            }

            RewardList rewardList = new RewardList();
            retval.Add(rewardList);
            long quantity = MathUtils.LongRange(spawnItem.MinQuantity, spawnItem.MaxQuantity, rand);

            ItemGenArgs igd = new ItemGenArgs()
            {
                ItemTypeId = spawnItem.EntityId,
                Level = rollLootArgs.Level,
                QualityTypeId = rollLootArgs.QualityTypeId,
                Quantity = 1,
            };

            if (itype.CanStack())
            {
                Reward rew = new Reward();
                rew.EntityId = spawnItem.EntityId;
                rew.EntityTypeId = EntityTypes.Item;
                rew.Quantity = 1;
                rew.QualityTypeId = rollLootArgs.QualityTypeId;
                rew.Level = rollLootArgs.Level;
                rewardList.Rewards.Add(rew);

                rew.ExtraData = _itemGenService.Generate(rand, igd);
                rew.Quantity = rollLootArgs.QualityTypeId;
            }
            else
            {
                for (int i = 0; i < quantity; i++)
                {
                    Reward rew = new Reward();
                    rew.EntityId = spawnItem.EntityId;
                    rew.EntityTypeId = EntityTypes.Item;
                    rew.Quantity = 1;
                    rew.QualityTypeId = rollLootArgs.QualityTypeId;
                    rew.Level = rollLootArgs.Level;
                    rewardList.Rewards.Add(rew);

                    rew.ExtraData = _itemGenService.Generate(rand, igd);
                }
            }
            return retval;
        }
    }
}


