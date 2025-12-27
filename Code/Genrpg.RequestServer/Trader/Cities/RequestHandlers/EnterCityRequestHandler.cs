using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Trader.Travel.Entities;
using Genrpg.RequestServer.Trader.Travel.Services;
using Genrpg.Shared.Trader.Cities.WebApi;

namespace Genrpg.RequestServer.Trader.Cities.RequestHandlers
{
    public class EnterCityRequestHandler : BaseClientUserRequestHandler<EnterCityRequest>
    {
        protected ICaravanPositionService _positionService = null;
        protected override async Task InnerHandleMessage(WebContext context, EnterCityRequest request, CancellationToken token)
        {
            EnterCityArgs args = new EnterCityArgs()
            {
                CityId = request.CityId,
            };

            context.AddResponse(await _positionService.EnterCity(context, args));

        }
    }
}
