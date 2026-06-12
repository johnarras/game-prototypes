using OxDb.PlayerServer.MessageHandlers;
using OxDb.ServerCore.Constants;
using OxDb.ServerCore.Core;
using OxDb.ServerCore.MainServer;

namespace OxDb.PlayerServer.Setup
{
    public class PlayerServerMain : BaseServer<ServerGameState, PlayerSetupService, IPlayerMessageHandler>
    {
        protected override bool UseInstanceId => false;
        protected override string GetBaseServerName() { return ServerNames.Player; }
    }
}

