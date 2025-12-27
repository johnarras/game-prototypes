using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Website.Messages.Error;
using System.Text;

namespace Genrpg.RequestServer.Services.GameClient
{
    public class GameClientWebService : IGameClientWebService
    {
        private IServerGameDataService _gameDataService = null;
        private ILogService _logService = null;
        private IWebServerService _loginServerService = null;
        private IHourlyUpdateService _hourlyUpdateService = null;
        private ITextSerializer _serializer = null;

        public async Task HandleUserClientRequest(WebContext context, string postData, CancellationToken token)
        {
            WebServerRequestSet commandSet = _serializer.Deserialize<WebServerRequestSet>(postData);

            await LoadLoggedInPlayer(context, commandSet.UserId, commandSet.SessionId);

            try
            {
                foreach (IWebRequest comm in commandSet.Requests)
                {
                    IGameClientRequestHandler handler = _loginServerService.GetGameClientRequestHandler(comm.GetType());
                    if (handler != null)
                    {
                        await handler.Execute(context, comm, token);
                    }
                }

                List<IWebResponse> errors = new List<IWebResponse>();

                foreach (IWebResponse response in context.GetResponseList())
                {
                    if (response is ErrorResponse error)
                    {
                        errors.Add(error);
                    }
                }

                if (errors.Count > 0)
                {
                    context.ClearResponses();
                    context.AddResponseRange(errors);
                    return;
                }

                await context.SaveAllOneTime();
            }
            catch (Exception e)
            {
                StringBuilder sb = new StringBuilder();
                foreach (IWebRequest req in commandSet.Requests)
                {
                    sb.Append(req.GetType().Name + " ");
                }
                sb.Append(" Exception: " + e.Message + "\n" + e.StackTrace);
                string errorMessage = "HandleUserClient." + sb.ToString();
                _logService.Exception(e, errorMessage);
                context.ShowError(errorMessage);
            }

            return;
        }

        private async Task LoadLoggedInPlayer(WebContext context, string userId, string sessionId)
        {
            await context.LoadUser(userId);

            if (context.acct == null || context.acct.SessionId != sessionId)
            {
                return;
            }

            context.user = await context.GetAsync<CoreUserData>();
            context.AddResponseRange(_gameDataService.GetClientSettings(context.user, false));
            await _hourlyUpdateService.CheckHourlyCurrencyUpdate(context, false);

            return;
        }

    }
}


