using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spawns.MessageHandlers
{
    public class DespawnObjectHandler : BaseMapObjectServerMapMessageHandler<DespawnObject>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, DespawnObject message)
        {
            obj.AddMessage(message);
        }
    }
}


