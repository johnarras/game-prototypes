using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;

namespace Genrpg.RequestServer.Services.GameClient
{
    public class GameClientWebService : IGameClientWebService
    {
        private IGameDataService _gameDataService = null;
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

                foreach (IWebResponse response in context.Responses.GetResponses())
                {
                    if (response is ErrorResponse error)
                    {
                        errors.Add(error);
                    }
                }

                if (errors.Count > 0)
                {
                    context.Responses.Clear();
                    context.Responses.AddRange(errors);
                    return;
                }

                await context.SaveAll();
            }
            catch (Exception e)
            {
                string errorMessage = "HandleLoginCommand." + commandSet.Requests.Select(x => x.GetType().Name + " ").ToList();
                _logService.Exception(e, errorMessage);
                context.ShowError(errorMessage);
            }

            return;
        }

        private async Task LoadLoggedInPlayer(WebContext context, string userId, string sessionId)
        {
            await context.LoadUser(userId);

            if (context.user == null || context.user.SessionId != sessionId)
            {
                return;
            }

            _gameDataService.GetClientSettings(context.Responses, context.user, false);
            await _hourlyUpdateService.CheckHourlyUpdate(context);

            return;
        }

    }
}
