using OxDb.RequestServer.ClientUserRequests.RequestHandlers;
using OxDb.RequestServer.Core;
using OxDb.RequestServer.Trader.Travel.Services;
using OxDb.SharedGame.Trader.Travel.WebApi;

namespace OxDb.RequestServer.Trader.Travel.RequestHandlers
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
