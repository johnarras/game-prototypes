using OxDb.RequestServer.ClientUserRequests.RequestHandlers;
using OxDb.RequestServer.Core;
using OxDb.RequestServer.Minigames.Games.Services;
using OxDb.SharedGame.Minigames.Games.WebApi;

namespace OxDb.RequestServer.Minigames.Games.RequestHandlers
{
    public class EndMinigameRequestHandler : BaseClientUserRequestHandler<EndMinigameRequest>
    {
        private IServerMingiameService _minigameService = null;
        protected override async Task InnerHandleMessage(WebContext context, EndMinigameRequest request, CancellationToken token)
        {
            await _minigameService.EndMinigame(context, request.MinigameTypeId, request.WonGame);
            await Task.CompletedTask;
        }
    }
}
