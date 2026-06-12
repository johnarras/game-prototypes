using OxDb.ServerCore.Config;
using OxDb.SharedCore.Core.Entities;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;

namespace OxDb.ServerCore.Core
{
    public class ServerGameState : GameState
    {
        public ServerGameState()
        {

        }

        public ServerGameState(IServerConfig configIn, ILogService logService)
        {
            IServerConfig config = configIn;
            IReflectionService reflectionService = new ReflectionService(logService);
            _loc = new ServiceLocator(logService, reflectionService, new GameData());
            _loc.Set(config);
        }
    }
}


