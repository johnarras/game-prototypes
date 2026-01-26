
using Genrpg.RequestServer.AuthRequests.AccountAuthRequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;

namespace Genrpg.RequestServer.Services.AccountAuth
{
    public class AccountAuthWebService : IAccountAuthWebService
    {
        private IWebServerService _webServerService = null;
        private ILogService _logService = null;

        public async Task HandleAccountAuthRequest(WebContext context, WebServerRequestSet requestSet, CancellationToken token)
        {
            try
            {

                foreach (IAccountAuthRequest authCommand in requestSet.Requests)
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



