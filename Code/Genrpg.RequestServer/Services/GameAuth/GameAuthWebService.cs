
using Genrpg.RequestServer.AuthRequests.GameAuthRequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;

namespace Genrpg.RequestServer.Services.GameAuth
{
    public class GameAuthWebService : IGameAuthWebService
    {
        private IWebServerService _webServerService = null;
        private ITextSerializer _serializer = null;
        private ILogService _logService = null;

        public async Task HandleGameAuthRequest(WebContext context, string postData, CancellationToken token)
        {
            try
            {
                WebServerRequestSet commandSet = _serializer.Deserialize<WebServerRequestSet>(postData);

                foreach (IGameAuthRequest authCommand in commandSet.Requests)
                {
                    IGameAuthRequestHandler handler = _webServerService.GetGameAuthRequestHandler(authCommand.GetType());

                    if (handler != null)
                    {
                        await handler.Execute(context, authCommand, token);
                    }
                }
                await context.SaveAllOneTime();
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "GameAuth.HandleRequest");
            }
        }
    }
}



