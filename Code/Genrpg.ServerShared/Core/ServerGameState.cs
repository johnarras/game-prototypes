using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Logging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;

namespace Genrpg.ServerShared.Core
{
    public class ServerGameState : GameState
    {
        public ServerGameState()
        {

        }

        public ServerGameState(IServerConfig configIn)
        {
            IServerConfig config = configIn;
            ILogService logService = new ServerLogService(configIn);
            IReflectionService reflectionService = new ReflectionService();
            _loc = new ServiceLocator(logService, reflectionService, new GameData());
            _loc.Set(config);
        }
    }
}


