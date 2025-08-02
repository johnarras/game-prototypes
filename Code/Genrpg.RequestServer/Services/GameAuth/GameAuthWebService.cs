
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.AuthRequests.GameAuthRequestHandlers;

namespace Genrpg.RequestServer.Services.GameAuth
{
    public class GameAuthWebService : IGameAuthWebService
    {
        private IWebServerService _webServerService = null;
        private ITextSerializer _serializer = null;

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
                await context.SaveAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

