using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Trader.Travel.Entities;
using Genrpg.RequestServer.Trader.Travel.Services;
using Genrpg.Shared.Trader.Travel.WebApi;

namespace Genrpg.RequestServer.Trader.Travel.RequestHandlers
{
    public class TravelRequestHandler : BaseClientUserRequestHandler<TravelRequest>
    {

        private IServerTravelService _travelService = null;
        protected override async Task InnerHandleMessage(WebContext context, TravelRequest request, CancellationToken token)
        {

            TravelArgs args = new TravelArgs()
            {
            };


            context.AddResponse(await _travelService.Travel(context, args));
        }
    }
}
