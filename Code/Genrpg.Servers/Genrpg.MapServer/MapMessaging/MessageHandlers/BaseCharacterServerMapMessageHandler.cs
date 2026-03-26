using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.MapMessages.Interfaces;

namespace Genrpg.MapServer.MapMessaging.MessageHandlers
{
    public abstract class BaseCharacterServerMapMessageHandler<TMapMessage> : BaseServerMapMessageHandler<Character, TMapMessage>
        where TMapMessage : class, IMapMessage, new()
    {
    }
}


