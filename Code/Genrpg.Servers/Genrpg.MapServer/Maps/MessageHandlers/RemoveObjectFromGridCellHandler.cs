using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.Maps.Messaging;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Maps.MessageHandlers
{
    public class RemoveObjectFromGridCellHandler : BaseMapObjectServerMapMessageHandler<RemoveObjectFromGridCell>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, RemoveObjectFromGridCell message)
        {
            _objectManager.FinalRemoveObjectFromOldGrid(rand, obj, message.GridData, message.GridItem);
        }
    }
}


