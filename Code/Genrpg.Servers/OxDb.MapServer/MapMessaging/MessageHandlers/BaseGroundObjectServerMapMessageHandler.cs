using OxDb.SharedGame.GroundObjects.MapObjects;
using OxDb.SharedGame.MapMessages.Interfaces;

namespace OxDb.MapServer.MapMessaging.MessageHandlers
{
    public abstract class BaseGroundObjectServerMapMessageHandler<TMapMessage> : BaseServerMapMessageHandler<GroundObject, TMapMessage>
        where TMapMessage : class, IMapMessage, new()
    {
    }
}


