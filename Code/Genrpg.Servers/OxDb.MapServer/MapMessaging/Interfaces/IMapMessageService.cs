using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.MapMessages.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;
using System.Collections.Generic;
using System.Threading;

namespace OxDb.MapServer.MapMessaging.Interfaces
{
    public interface IMapMessageService : IInitializable
    {
        void Init(CancellationToken token);
        void SendMessage(MapObject mapObject, IMapMessage message, float delaySeconds = 0);

        void SendMessageNear(MapObject obj, IMapMessage message,
            float dist = MessageConstants.DefaultGridDistance,
            bool playersOnly = true,
            float delaySec = 0, List<long> filters = null);
        void UpdateGameData(IGameData gameData);
        void SendMessageToAllPlayers(IMapApiMessage message);
    }
}


