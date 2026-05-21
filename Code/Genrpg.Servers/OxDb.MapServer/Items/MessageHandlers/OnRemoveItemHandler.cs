using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Inventory.Messages;
using OxDb.SharedGame.MapObjects.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Items.MessageHandlers
{
    public class OnRemoveItemHandler : BaseMapObjectServerMapMessageHandler<OnRemoveItem>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, OnRemoveItem message)
        {
            obj.AddMessage(message);
        }
    }
}


