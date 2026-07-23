using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.Vendors.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Inventory.Messages;
using OxDb.SharedGame.MapObjects.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Vendors.MessageHandlers
{
    public class BuyItemHandler : BaseMapObjectServerMapMessageHandler<BuyItem>
    {
        private IVendorService _vendorService = null!;
        protected override async ValueTask InnerProcess(MapObject obj, BuyItem message)
        {
            await _vendorService.BuyItem(obj, message);
        }
    }
}


