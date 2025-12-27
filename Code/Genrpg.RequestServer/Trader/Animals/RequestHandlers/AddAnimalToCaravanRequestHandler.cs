using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Animals.WebApi;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using Genrpg.Shared.Trader.Stats.PlayerData;

namespace Genrpg.RequestServer.Trader.Animals.RequestHandlers
{
    public class AddAnimalToCaravanRequestHandler : BaseClientUserRequestHandler<AddAnimalToCaravanRequest>
    {
        protected ICaravanService _caravanService = null;
        protected override async Task InnerHandleMessage(WebContext context, AddAnimalToCaravanRequest request, CancellationToken token)
        {

            CoreUserData userData = await context.GetAsync<CoreUserData>();
            CaravanData caravanData = await context.GetAsync<CaravanData>();
            HoldingsData holdingsData = await context.GetAsync<HoldingsData>();
            TraderStatData statData = await context.GetAsync<TraderStatData>();

            context.AddResponse(_caravanService.AddAnimalToCaravan(userData, caravanData, holdingsData, statData, request.AnimalTypeId, false));

        }
    }
}
