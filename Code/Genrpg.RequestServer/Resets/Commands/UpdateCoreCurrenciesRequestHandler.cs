using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Entities;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.Shared.UserEnergy.WebApi;

namespace Genrpg.RequestServer.Resets.Commands
{
    public class UpdateCoreCurrenciesRequestHandler : BaseClientUserRequestHandler<UpdateCoreCurrenciesRequest>
    {

        private IHourlyUpdateService _hourlyUpdateService = null;
        protected override async Task InnerHandleMessage(WebContext context, UpdateCoreCurrenciesRequest request, CancellationToken token)
        {
            await _hourlyUpdateService.CheckHourlyCurrencyUpdate(context, new HourlyResetArgs() { OnLogin = false });
        }
    }
}


