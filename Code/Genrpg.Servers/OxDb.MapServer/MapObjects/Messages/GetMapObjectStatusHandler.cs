using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.Vendors.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.MapObjects.Messages
{
    public class GetMapObjectStatusHandler : BaseMapObjectServerMapMessageHandler<GetMapObjectStatus>
    {

        private IVendorService _vendorService = null;

        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, GetMapObjectStatus message)
        {
            OnGetMapObjectStatus result = new OnGetMapObjectStatus() { ObjId = message.ObjId };
            if (_objectManager.GetObject(message.ObjId, out MapObject mapObject))
            {
                result.Addons = mapObject.GetAddons();
                _vendorService.UpdateItems(rand.Rand, mapObject);
            }

            _messageService.SendMessage(obj, result);
        }
    }
}


