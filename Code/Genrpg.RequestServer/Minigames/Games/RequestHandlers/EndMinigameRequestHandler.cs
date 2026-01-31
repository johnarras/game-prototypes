using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Minigames.Games.Services;
using Genrpg.Shared.Minigames.Games.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.RequestServer.Minigames.Games.RequestHandlers
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
