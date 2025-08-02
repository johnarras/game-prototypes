
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.AuthRequests.AccountAuthRequestHandlers;

namespace Genrpg.RequestServer.Services.AccountAuth
{
    public class AccountAuthWebService : IAccountAuthWebService
    {
        private IWebServerService _webServerService = null;
        private ITextSerializer _serializer = null;

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
                await context.SaveAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

