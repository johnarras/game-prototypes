using OxDb.InstanceServer.MessageHandlers;
using OxDb.ServerCore.Constants;
using OxDb.ServerCore.Core;
using OxDb.ServerCore.MainServer;

namespace OxDb.InstanceServer.Setup
{
    public class InstanceServerMain : BaseServer<ServerGameState, InstanceSetupService, IInstanceMessageHandler>
    {

        protected override string GetBaseServerName()
        {
            return ServerNames.InstanceManager;
        }
    }
}

