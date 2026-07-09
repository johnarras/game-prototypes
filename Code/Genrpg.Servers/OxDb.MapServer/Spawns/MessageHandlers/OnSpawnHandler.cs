using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spawns.MessageHandlers
{
    public class OnSpawnHandler : BaseMapObjectServerMapMessageHandler<OnSpawn>
    {
        protected override async ValueTask InnerProcess(MapObject obj, OnSpawn message)
        {
            obj.AddMessage(message);
        }
    }
}


