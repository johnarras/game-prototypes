using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Items.MessageHandlers
{
    public class OnAddItemHandler : BaseMapObjectServerMapMessageHandler<OnAddItem>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, OnAddItem message)
        {
            obj.AddMessage(message);
        }
    }
}


