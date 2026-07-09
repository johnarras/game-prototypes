using OxDb.RequestServer.Core;
using OxDb.RequestServer.GameClientRequests.RequestHandlers;
using OxDb.RequestServer.Maps;
using OxDb.RequestServer.PlayerData.Services;
using OxDb.ServerGame.Maps;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Website.Requests.Core;
using OxDb.SharedCore.Website.Requests.Interfaces;
using OxDb.SharedCore.Website.Responses.Errors;
using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.Core.PlayerData;
using System.Text;

namespace OxDb.RequestServer.GameClientRequests.Services
{
    public class GameClientRequestService : IGameClientRequestService
    {
        private ILogService _logService = null;
        private ILoadPlayerDataService _loadPlayerDataService = null;
        private IMapDataService _mapDataService = null!;

        private MapStubList _mapStubs { get; set; } = new MapStubList();

        public async Task Initialize(CancellationToken token)
        {
            _mapStubs.Stubs = await _mapDataService.GetMapStubs();
            await Task.CompletedTask;
        }

        private SetupDictionaryContainer<Type, IGameClientRequestHandler> _clientCommandHandlers = new SetupDictionaryContainer<Type, IGameClientRequestHandler>();

        public async Task HandleUserClientRequest(WebContext context, WebServerRequestSet requestSet, string tokenUserId, string gameSessionId, CancellationToken token)
        {

            try
            {
                if (!await LoadLoggedInPlayer(context, tokenUserId, gameSessionId, requestSet.GameUserId))
                {
                    return;
                }

                foreach (IWebRequest comm in requestSet.Requests)
                {
                    IGameClientRequestHandler handler = GetGameClientRequestHandler(comm.GetType());
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

                bool didSave = await context.SaveAllOneTime();

                if (!didSave)
                {
                    context.ClearResponses();
                    context.AddResponse(new ErrorResponse() { Error = "Error. Please try again in a few minutes." });
                }
            }
            catch (Exception e)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(" Exception: " + e.Message);
                string errorMessage = sb.ToString();
                _logService.Exception(e, errorMessage);
                context.ShowError(errorMessage);
            }

            return;
        }

        private async Task<bool> LoadLoggedInPlayer(WebContext context, string tokenUserId, string gameSessionId, string requestListUserId)
        {
            if (tokenUserId != requestListUserId)
            {
                context.ShowError("Auth User Id does not match Request User Id");
                return false;
            }

            context.SetGameUserId(tokenUserId);

            CoreData coreData = await context.GetAsync<CoreData>();

            if (coreData.GameSessionId != gameSessionId)
            {
                context.ShowError("You are logged in on another device.");
                return false;
            }


            await _loadPlayerDataService.UpdatePlayerAfterLoginOrLoad(context, false);

            return true;
        }

        private IGameClientRequestHandler GetGameClientRequestHandler(Type type)
        {
            if (_clientCommandHandlers.TryGetValue(type, out IGameClientRequestHandler commandHandler))
            {
                return commandHandler;
            }

            return null;
        }

        public MapStubList GetMapStubs()
        {
            return _mapStubs;
        }

        public async Task ResetRequestHandlers()
        {
            foreach (IGameClientRequestHandler handler in _clientCommandHandlers.GetDict().Values)
            {
                await handler.Reset();
            }
        }
    }
}


