using OxDb.RequestServer.ClientUserRequests.RequestHandlers;
using OxDb.RequestServer.Core;
using OxDb.RequestServer.Trader.Travel.Entities;
using OxDb.RequestServer.Trader.Travel.Services;
using OxDb.SharedGame.Trader.Travel.WebApi;

namespace OxDb.RequestServer.Trader.Travel.RequestHandlers
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
