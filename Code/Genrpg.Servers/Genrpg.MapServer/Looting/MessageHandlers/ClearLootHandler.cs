using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Looting.MessageHandlers
{
    public class ClearLootHandler : BaseMapObjectServerMapMessageHandler<ClearLoot>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, ClearLoot message)
        {
            obj.AddMessage(message);
        }
    }
}


