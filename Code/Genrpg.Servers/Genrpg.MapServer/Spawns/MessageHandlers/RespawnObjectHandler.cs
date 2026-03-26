using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.Maps.Messaging;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spawns.MessageHandlers
{
    public class RespawnObjectHandler : BaseMapObjectServerMapMessageHandler<RespawnObject>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, RespawnObject message)
        {
            _objectManager.SpawnObject(rand, message.Spawn);
        }
    }
}


