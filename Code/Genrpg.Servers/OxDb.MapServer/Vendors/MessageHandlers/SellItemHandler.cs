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

        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, SellItem message)
        {
            _vendorService.SellItem(rand.Rand, obj, message);
        }
    }
}


