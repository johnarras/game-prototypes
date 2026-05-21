using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Inventory.Messages;
using OxDb.SharedGame.MapObjects.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Items.MessageHandlers
{
    public class OnAddItemHandler : BaseMapObjectServerMapMessageHandler<OnAddItem>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, OnAddItem message)
        {
            obj.AddMessage(message);
        }
    }
}


