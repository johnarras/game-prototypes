using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Trader.Travel.Entities;
using Genrpg.RequestServer.Trader.Travel.Services;
using Genrpg.Shared.Trader.Roads.WebApi;

namespace Genrpg.RequestServer.Trader.Roads.RequestHandlers
{
    public class EnterRoadRequestHandler : BaseClientUserRequestHandler<EnterRoadRequest>
    {
        protected IServerCaravanService _positionService = null;
        protected override async Task InnerHandleMessage(WebContext context, EnterRoadRequest request, CancellationToken token)
        {
            EnterRoadArgs args = new EnterRoadArgs()
            {
                RoadId = request.RoadId,
            };

            context.AddResponse(await _positionService.EnterRoad(context, args));

        }
    }
}
