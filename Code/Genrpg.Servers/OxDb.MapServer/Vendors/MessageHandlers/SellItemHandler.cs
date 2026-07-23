using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.Vendors.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Inventory.Messages;
using OxDb.SharedGame.MapObjects.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Vendors.MessageHandlers
{
    public class SellItemHandler : BaseMapObjectServerMapMessageHandler<SellItem>
    {

        private IVendorService _vendorService = null!;

        protected override async ValueTask InnerProcess(MapObject obj, SellItem message)
        {
            await _vendorService.SellItem(obj, message);
        }
    }
}


