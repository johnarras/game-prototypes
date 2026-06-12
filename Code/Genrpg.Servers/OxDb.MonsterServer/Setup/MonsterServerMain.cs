using OxDb.MonsterServer.MessageHandlers;
using OxDb.ServerCore.Constants;
using OxDb.ServerCore.Core;
using OxDb.ServerCore.MainServer;

namespace OxDb.MonsterServer.Setup
{
    public class MonsterServerMain : BaseServer<ServerGameState, MonsterSetupService, IMonsterMessageHandler>
    {
        protected override bool UseInstanceId => false;
        protected override string GetBaseServerName() { return ServerNames.Monster; }
    }
}

