using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.Maps.Messaging;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spawns.MessageHandlers
{
    public class RespawnObjectHandler : BaseMapObjectServerMapMessageHandler<RespawnObject>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, RespawnObject message)
        {
            _objectManager.SpawnObject(rand.Rand, message.Spawn);
        }
    }
}


