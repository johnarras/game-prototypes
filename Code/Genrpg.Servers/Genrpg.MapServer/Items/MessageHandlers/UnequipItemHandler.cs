using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Items.MessageHandlers
{
    public class UnequipItemHandler : BaseUnitServerMapMessageHandler<UnequipItem>
    {

        private IInventoryService _inventoryService = null;

        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, UnequipItem message)
        {
            if (!_inventoryService.UnequipItem(unit, message.ItemId))
            {
                pack.SendError(unit, "That item isn't equipped");
                return;
            }
        }
    }
}


