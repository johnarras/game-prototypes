using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.Vendors.Services;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.MapObjects.Messages
{
    public class GetMapObjectStatusHandler : BaseMapObjectServerMapMessageHandler<GetMapObjectStatus>
    {

        private IVendorService _vendorService = null;

        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, GetMapObjectStatus message)
        {
            OnGetMapObjectStatus result = new OnGetMapObjectStatus() { ObjId = message.ObjId };
            if (_objectManager.GetObject(message.ObjId, out MapObject mapObject))
            {
                result.Addons = mapObject.GetAddons();
                _vendorService.UpdateItems(rand, mapObject);
            }

            _messageService.SendMessage(obj, result);
        }
    }
}


