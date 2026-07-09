using OxDb.RequestServer.Core;
using OxDb.RequestServer.GameClientRequests.RequestHandlers;
using OxDb.RequestServer.Resets.Entities;
using OxDb.RequestServer.Resets.Services;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Trader.Camping.WebApi;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.Services;

namespace OxDb.RequestServer.Trader.Travel.RequestHandlers
{
    public class CampRequestHandler : BaseClientUserRequestHandler<CampRequest>
    {
        private ICaravanService _caravanService = null;
        private IHourlyUpdateService _hourlyUpdateService = null;
        protected override async Task InnerHandleMessage(WebContext context, CampRequest request, CancellationToken token)
        {

            CoreData coreData = await context.GetAsync<CoreData>();

            CaravanPosition position = await _caravanService.GetPosition(context);

            await _hourlyUpdateService.CheckHourlyCurrencyUpdates(context, new HourlyResetArgs()
            {
                OnLogin = false,
                IsCamping = true,
                InCity = position.GetCurrentCity() != null,
            });

        }
    }
}
