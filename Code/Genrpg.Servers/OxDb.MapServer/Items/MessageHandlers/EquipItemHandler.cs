using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Inventory.Messages;
using OxDb.SharedGame.Inventory.Services;
using System.Threading.Tasks;

namespace OxDb.MapServer.Items.MessageHandlers
{
    public class EquipItemHandler : BaseCharacterServerMapMessageHandler<EquipItem>
    {

        private IInventoryService _inventoryService = null;
        protected override async ValueTask InnerProcess(Character ch, EquipItem message)
        {
            if (!_inventoryService.EquipItem(ch, message.ItemId, message.EquipSlot))
            {
                ch.SendError("You can't equip that there");
                return;
            }
        }
    }
}


