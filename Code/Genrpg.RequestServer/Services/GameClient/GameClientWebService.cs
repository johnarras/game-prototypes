using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayerData.Services;
using Genrpg.RequestServer.Resets.Entities;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Logging.Interfaces;
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
        private ILoadPlayerDataService _loadPlayerDataService = null;

        public async Task HandleUserClientRequest(WebContext context, WebServerRequestSet requestSet, CancellationToken token)
        {

            if (!await LoadLoggedInPlayer(context, requestSet.GameUserId, requestSet.GameUserId))
            {
                context.ShowError("Failed to load logged in user.");
                return;
            }

            try
            {
                foreach (IWebRequest comm in requestSet.Requests)
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
                foreach (IWebRequest req in requestSet.Requests)
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

        private async Task<bool> LoadLoggedInPlayer(WebContext context, string tokenUserId, string requestListUserId)
        {
            if (tokenUserId != requestListUserId)
            {
                return false;
            }

            context.SetGameUserId(tokenUserId);

            context.core = await context.GetAsync<CoreData>();

            await _loadPlayerDataService.UpdatePlayerAfterLoginOrLoad(context, false);

            return true;
        }

    }
}


