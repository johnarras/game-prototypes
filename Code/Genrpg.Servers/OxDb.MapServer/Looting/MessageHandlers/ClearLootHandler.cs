using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Loot.Messages;
using OxDb.SharedGame.MapObjects.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Looting.MessageHandlers
{
    public class ClearLootHandler : BaseMapObjectServerMapMessageHandler<ClearLoot>
    {
        protected override async ValueTask InnerProcess(MapObject obj, ClearLoot message)
        {
            obj.AddMessage(message);
        }
    }
}


