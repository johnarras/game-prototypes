using Genrpg.RequestServer.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MobileGame.Constants;
using Genrpg.Shared.PlayMultiplier.Services;
using Genrpg.Shared.PlayMultiplier.Settings;
using Genrpg.Shared.PlayMultiplier.WebApi;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Utils;

namespace Genrpg.RequestServer.PlayMultiplier.Services
{
    public class ServerPlayMultService : IServerPlayMultService
    {
        private ISharedPlayMultService _sharedPlayMultService = null;
        private ICaravanService _caravanService = null;
        private IGameData _gameData = null;       
        public async Task SetPlayMult(WebContext context, int newPlayMult)
        {
            CoreData coreData = await context.GetAsync<CoreData>();

            PlayMultSettings multSettings = _gameData.Get<PlayMultSettings>(coreData);

            long level = coreData.Level;

            if (coreData.Level < 1)
            {
                coreData.Level = 1;
            }
            int maxMult = _sharedPlayMultService.GetMaxMult(coreData);

            long totalSpend = 0;
            for (int i = 0; i < coreData.TravelDayCurrencies.Count(); i++)
            {
                if (coreData.TravelDayCurrencies[i] < 0)
                {
                    totalSpend += Math.Abs(coreData.TravelDayCurrencies[i]);
                }
            }

            newPlayMult = MathUtil.Clamp(MobileGameConstants.MinPlayMult, newPlayMult, maxMult);

            coreData.Vars[TraderVars.Mult] = newPlayMult;
            coreData.Vars[TraderVars.MultBonusSpeed] = (int)(newPlayMult * totalSpend * multSettings.ExtraDailyDistPerTotalCurrencySpend);

            context.AddResponse(new SetPlayMultResponse() { Success = true, NewPlayMult = newPlayMult, 
                MultBonusSpeed = coreData.Vars[TraderVars.MultBonusSpeed],
            });
        }
    }
}


