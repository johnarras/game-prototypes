using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.MapMessages.Interfaces;

namespace OxDb.MapServer.MapMessaging.MessageHandlers
{
    public abstract class BaseCharacterServerMapMessageHandler<TMapMessage> : BaseServerMapMessageHandler<Character, TMapMessage>
        where TMapMessage : class, IMapMessage, new()
    {
    }
}


