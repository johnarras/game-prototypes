using Genrpg.Shared.GroundObjects.MapObjects;
using Genrpg.Shared.MapMessages.Interfaces;

namespace Genrpg.MapServer.MapMessaging.MessageHandlers
{
    public abstract class BaseGroundObjectServerMapMessageHandler<TMapMessage> : BaseServerMapMessageHandler<GroundObject, TMapMessage>
        where TMapMessage : class, IMapMessage, new()
    {
    }
}


