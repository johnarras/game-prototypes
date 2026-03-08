using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Linq;
using System.Threading.Tasks;
namespace Genrpg.Shared.Inventory.Helpers
{
    public class ItemRewardHelper : IRewardHelper
    {

        public long HelperKey => EntityTypes.Item;
        private IInventoryService _inventoryService = null;
        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
            Item startItem = extraData as Item;
            if (startItem != null)
            {
                _inventoryService.AddItem(obj, startItem, true);
                return true;
            }
            return true;
        }

        public long GetQuantity(MapObject obj, long entityId)
        {
            InventoryData idata = obj.Get<InventoryData>();

            return idata.GetItemsByItemTypeId(entityId).Sum(x => x.Quantity);
        }

    }
}


