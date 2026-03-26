using Genrpg.MapServer.Units.Services;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.MapServer.MapMessaging.MessageHandlers
{
    public abstract class BaseUnitServerMapMessageHandler<TMapMessage> : BaseServerMapMessageHandler<Unit, TMapMessage>
        where TMapMessage : class, IMapMessage, new()
    {
        protected IServerUnitService _unitService = null;
    }
}


