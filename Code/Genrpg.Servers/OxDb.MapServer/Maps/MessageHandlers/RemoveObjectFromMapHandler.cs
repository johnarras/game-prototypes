using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.Maps.Messaging;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Maps.MessageHandlers
{
    public class RemoveObjectFromMapHandler : BaseMapObjectServerMapMessageHandler<RemoveObjectFromMap>
    {
        protected override async ValueTask InnerProcess(MapObject obj, RemoveObjectFromMap message)
        {
            _objectManager.RemoveObject(obj.Rand, obj.Id);
        }
    }
}


