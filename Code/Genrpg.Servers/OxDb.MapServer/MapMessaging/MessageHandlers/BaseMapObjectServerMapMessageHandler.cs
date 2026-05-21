using OxDb.SharedGame.MapMessages.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;

namespace OxDb.MapServer.MapMessaging.MessageHandlers
{
    public abstract class BaseMapObjectServerMapMessageHandler<TMapMessage> : BaseServerMapMessageHandler<MapObject, TMapMessage>
        where TMapMessage : class, IMapMessage, new()
    {
    }
}


