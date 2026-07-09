using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.Maps.Messaging;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Maps.MessageHandlers
{
    public class RemoveObjectFromGridCellHandler : BaseMapObjectServerMapMessageHandler<RemoveObjectFromGridCell>
    {
        protected override async ValueTask InnerProcess(MapObject obj, RemoveObjectFromGridCell message)
        {
            _objectManager.FinalRemoveObjectFromOldGrid(obj, message.GridData, message.GridItem);
        }
    }
}


