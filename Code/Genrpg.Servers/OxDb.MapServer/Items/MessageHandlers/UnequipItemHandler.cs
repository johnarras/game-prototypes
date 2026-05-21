using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Inventory.Messages;
using OxDb.SharedGame.Inventory.Services;
using OxDb.SharedGame.Units.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Items.MessageHandlers
{
    public class UnequipItemHandler : BaseUnitServerMapMessageHandler<UnequipItem>
    {

        private IInventoryService _inventoryService = null;

        protected override async Task InnerProcess(IRandomContainer rand, Unit unit, UnequipItem message)
        {
            if (!_inventoryService.UnequipItem(unit, message.ItemId))
            {
                unit.SendError("That item isn't equipped");
                return;
            }
        }
    }
}


