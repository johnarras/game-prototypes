using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.MessageHandlers;
using OxDb.ServerCore.Constants;
using OxDb.ServerCore.MainServer;

namespace OxDb.DataUtils.Services.Setup
{
    public class EditorServer : BaseServer<EditorGameState, EditorSetupService, IEditorMessageHandler>
    {
        protected override bool UseInstanceId => false;
        protected override string GetBaseServerName() { return ServerNames.Editor; }
    }
}
