
using Genrpg.RequestServer.AuthRequests.AccountAuthRequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;

namespace Genrpg.RequestServer.Services.AccountAuth
{
    public class AccountAuthWebService : IAccountAuthWebService
    {
        private IWebServerService _webServerService = null;
        private ITextSerializer _serializer = null;
        private ILogService _logService = null;

        public async Task HandleAccountAuthRequest(WebContext context, string postData, CancellationToken token)
        {
            try
            {
                WebServerRequestSet commandSet = _serializer.Deserialize<WebServerRequestSet>(postData);

                foreach (IAccountAuthRequest authCommand in commandSet.Requests)
                {
                    IAccountAuthRequestHandler handler = _webServerService.GetAccountAuthRquestHandler(authCommand.GetType());

                    if (handler != null)
                    {
                        await handler.Execute(context, authCommand, token);
                    }
                }
                await context.SaveAllOneTime();
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "AccountAuth.HandleRequest");
            }
        }
    }
}



