using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Entities;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Entities;
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
        private IRepositoryService _repoService = null;

        public async Task HandleUserClientRequest(WebContext context, string postData, CancellationToken token)
        {
            WebServerRequestSet commandSet = _serializer.Deserialize<WebServerRequestSet>(postData);

            if (!await LoadLoggedInPlayer(context, commandSet.GameUserId, commandSet.SessionId))
            {
                context.ShowError("Failed to load logged in user.");
                return;
            }

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

        private async Task<bool> LoadLoggedInPlayer(WebContext context, string userId, string sessionId)
        {
            GameAccount acct = await _repoService.Load<GameAccount>(userId);

            if (acct == null || acct.SessionId != sessionId)
            {
                return false;
            }

            context.SetAccount(acct);

            context.core = await context.GetAsync<CoreData>();
            context.AddResponseRange(_gameDataService.GetClientSettings(context.core, false));
            await _hourlyUpdateService.CheckHourlyCurrencyUpdate(context, new HourlyResetArgs() { OnLogin = false });

            return true;
        }

    }
}


