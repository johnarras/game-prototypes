
using OxDb.PlatformServer.AccountAuthRequests.RequestHandlers;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Website.Interfaces;
using OxDb.SharedCore.Website.Requests.Core;
using OxDb.SharedCore.Website.Responses.Core;

namespace OxDb.PlatformServer.Accounts.Services
{
    public class AccountAuthWebService : IAccountAuthWebService
    {
        private ILogService _logService = null;

        private SetupDictionaryContainer<Type, IAccountAuthRequestHandler> _accountAuthCommandHandlers = new SetupDictionaryContainer<Type, IAccountAuthRequestHandler>();

        public async Task HandleAccountAuthRequest(IWebContext context, WebServerRequestSet requestSet, CancellationToken token)
        {
            try
            {
                foreach (IAccountAuthRequest authCommand in requestSet.Requests)
                {
                    if (_accountAuthCommandHandlers.TryGetValue(authCommand.GetType(), out IAccountAuthRequestHandler handler))
                    {
                        await handler.Execute(context, authCommand, token);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "AccountAuth.HandleRequest");
            }
        }
    }
}



