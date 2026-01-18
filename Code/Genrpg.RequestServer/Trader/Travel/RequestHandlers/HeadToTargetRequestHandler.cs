using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Trader.Travel.Services;
using Genrpg.Shared.Trader.Travel.WebApi;

namespace Genrpg.RequestServer.Trader.Travel.RequestHandlers
{
    public class HeadToTargetRequestHandler : BaseClientUserRequestHandler<HeadToTargetRequest>
    {
        protected IServerCaravanService _positionService = null;
        protected override async Task InnerHandleMessage(WebContext context, HeadToTargetRequest request, CancellationToken token)
        {
            context.AddResponse(await _positionService.HeadToTarget(context, request, false));
        }
    }
}
