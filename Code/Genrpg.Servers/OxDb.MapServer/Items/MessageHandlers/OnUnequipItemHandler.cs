using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Inventory.Messages;
using OxDb.SharedGame.MapObjects.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Items.MessageHandlers
{
    public class OnUnequipItemHandler : BaseMapObjectServerMapMessageHandler<OnUnequipItem>
    {
        protected override async ValueTask InnerProcess(MapObject obj, OnUnequipItem message)
        {
            obj.AddMessage(message);
        }
    }
}


