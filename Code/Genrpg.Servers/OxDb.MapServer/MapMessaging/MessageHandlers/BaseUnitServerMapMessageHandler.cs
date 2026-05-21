using OxDb.MapServer.Units.Services;
using OxDb.SharedGame.MapMessages.Interfaces;
using OxDb.SharedGame.Units.Entities;

namespace OxDb.MapServer.MapMessaging.MessageHandlers
{
    public abstract class BaseUnitServerMapMessageHandler<TMapMessage> : BaseServerMapMessageHandler<Unit, TMapMessage>
        where TMapMessage : class, IMapMessage, new()
    {
        protected IServerUnitService _unitService = null;
    }
}


