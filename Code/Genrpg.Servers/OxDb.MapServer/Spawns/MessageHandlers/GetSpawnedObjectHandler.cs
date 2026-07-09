using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spawns.MessageHandlers
{
    public class GetSpawnedObjectHandler : BaseMapObjectServerMapMessageHandler<GetSpawnedObject>
    {
        protected override async ValueTask InnerProcess(MapObject obj, GetSpawnedObject message)
        {
            if (!_objectManager.GetObject(message.ObjId, out MapObject mapObj))
            {
                return;
            }

            _messageService.SendMessage(mapObj, new SendSpawn() { ToObjId = obj.Id });
        }
    }
}


