using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;

namespace Genrpg.MapServer.MapMessaging.MessageHandlers
{
    public abstract class BaseMapObjectServerMapMessageHandler<TMapMessage> : BaseServerMapMessageHandler<MapObject, TMapMessage>
        where TMapMessage : class, IMapMessage, new()
    {
    }
}


