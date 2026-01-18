using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Entities;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Camping.WebApi;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;

namespace Genrpg.RequestServer.Trader.Travel.RequestHandlers
{
    public class CampRequestHandler : BaseClientUserRequestHandler<CampRequest>
    {
        private ICaravanService _caravanService = null;
        private IHourlyUpdateService _hourlyUpdateService = null;
        protected override async Task InnerHandleMessage(WebContext context, CampRequest request, CancellationToken token)
        {

            CoreData coreData = await context.GetAsync<CoreData>();


            CaravanPosition position = _caravanService.GetPosition(coreData);

            await _hourlyUpdateService.CheckHourlyCurrencyUpdate(context, new HourlyResetArgs()
            {
                OnLogin = false,
                IsCamping = true,
                InCity = position.GetCurrentCity() != null,
            });

        }
    }
}
