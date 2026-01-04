using Genrpg.RequestServer.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.MobileGame.Constants;
using Genrpg.Shared.PlayMultiplier.Services;
using Genrpg.Shared.PlayMultiplier.WebApi;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Utils;

namespace Genrpg.RequestServer.PlayMultiplier.Services
{
    public class ServerPlayMultService : IServerPlayMultService
    {
        private ISharedPlayMultService _sharedPlayMultService = null;
        private ICaravanService _caravanService = null;
        public async Task SetPlayMult(WebContext context, long newPlayMult)
        {
            CoreData coreData = await context.GetAsync<CoreData>();

            long level = coreData.Level;

            if (coreData.Level < 1)
            {
                coreData.Level = 1;
            }

            newPlayMult = MathUtils.Clamp(MobileGameConstants.MinPlayMult, newPlayMult, _sharedPlayMultService.GetMaxMult(coreData));

            coreData.Vars[TraderVars.Mult] = newPlayMult;

            _caravanService.UpdateCoreStatsFromCaravan(coreData, await context.GetAsync<CaravanData>(), await context.GetAsync<TraderStatData>());
            context.AddResponse(new SetPlayMultResponse() { Success = true, NewPlayMult = newPlayMult });
        }
    }
}


