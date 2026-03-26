using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Items.MessageHandlers
{
    public class EquipItemHandler : BaseCharacterServerMapMessageHandler<EquipItem>
    {

        private IInventoryService _inventoryService = null;
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, EquipItem message)
        {
            if (!_inventoryService.EquipItem(ch, message.ItemId, message.EquipSlot))
            {
                pack.SendError(ch, "You can't equip that there");
                return;
            }
        }
    }
}


