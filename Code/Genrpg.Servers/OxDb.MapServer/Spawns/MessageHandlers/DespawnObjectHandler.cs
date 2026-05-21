using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spawns.MessageHandlers
{
    public class DespawnObjectHandler : BaseMapObjectServerMapMessageHandler<DespawnObject>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, DespawnObject message)
        {
            obj.AddMessage(message);
        }
    }
}


