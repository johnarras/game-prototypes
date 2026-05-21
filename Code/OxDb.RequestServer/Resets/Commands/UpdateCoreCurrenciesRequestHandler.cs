
using OxDb.RequestServer.ClientUserRequests.RequestHandlers;
using OxDb.RequestServer.Core;
using OxDb.RequestServer.Resets.Entities;
using OxDb.RequestServer.Resets.Services;
using OxDb.SharedGame.UserEnergy.WebApi;

namespace OxDb.RequestServer.Resets.Commands
{
    public class UpdateCoreCurrenciesRequestHandler : BaseClientUserRequestHandler<UpdateCoreCurrenciesRequest>
    {

        private IHourlyUpdateService _hourlyUpdateService = null;
        protected override async Task InnerHandleMessage(WebContext context, UpdateCoreCurrenciesRequest request, CancellationToken token)
        {
            await _hourlyUpdateService.CheckHourlyCurrencyUpdates(context, new HourlyResetArgs() { OnLogin = false });
        }
    }
}


