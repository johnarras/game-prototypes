using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Items.MessageHandlers
{
    public class OnRemoveItemHandler : BaseMapObjectServerMapMessageHandler<OnRemoveItem>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, OnRemoveItem message)
        {
            obj.AddMessage(message);
        }
    }
}


