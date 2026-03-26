using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.Vendors.Services;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Vendors.MessageHandlers
{
    public class BuyItemHandler : BaseMapObjectServerMapMessageHandler<BuyItem>
    {
        private IVendorService _vendorService = null!;
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, BuyItem message)
        {
            _vendorService.BuyItem(rand, obj, message);
        }
    }
}


