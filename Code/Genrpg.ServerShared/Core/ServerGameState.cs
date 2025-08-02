
using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using Genrpg.ServerShared.Analytics.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Logging;
using Genrpg.Shared.Analytics.Services;
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
            ITextSerializer textSerializer = new NewtonsoftTextSerializer();
            ILogService logService = new ServerLogService(configIn, textSerializer);
            IAnalyticsService analyticsService = new ServerAnalyticsService(configIn, textSerializer);
            loc = new ServiceLocator(textSerializer, logService, analyticsService, new GameData());
            loc.Set(config);
        }  
    }
}
