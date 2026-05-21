using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.Maps.Messaging;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Maps.MessageHandlers
{
    public class RemoveObjectFromMapHandler : BaseMapObjectServerMapMessageHandler<RemoveObjectFromMap>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, RemoveObjectFromMap message)
        {
            _objectManager.RemoveObject(rand.Rand, obj.Id);
        }
    }
}


