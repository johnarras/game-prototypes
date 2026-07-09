using OxDb.RequestServer.Core;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.MobileGame.Constants;
using OxDb.SharedGame.PlayMultiplier.Services;
using OxDb.SharedGame.PlayMultiplier.Settings;
using OxDb.SharedGame.PlayMultiplier.WebApi;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Constants;

namespace OxDb.RequestServer.PlayMultiplier.Services
{
    public interface IServerPlayMultService : IInjectable
    {
        ValueTask SetPlayMult(WebContext context, int newPlayMult);
    }
    public class ServerPlayMultService : IServerPlayMultService
    {
        private ISharedPlayMultService _sharedPlayMultService = null;
        private ICaravanService _caravanService = null;
        private IGameData _gameData = null;
        public async ValueTask SetPlayMult(WebContext context, int newPlayMult)
        {
            CoreData coreData = await context.GetAsync<CoreData>();

            PlayMultSettings multSettings = _gameData.Get<PlayMultSettings>(coreData);

            long level = coreData.Level;

            if (coreData.Level < 1)
            {
                coreData.Level = 1;
            }
            int maxMult = await _sharedPlayMultService.GetMaxMult(context);

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

            context.AddResponse(new SetPlayMultResponse()
            {
                Success = true,
                NewPlayMult = newPlayMult,
                MultBonusSpeed = coreData.Vars[TraderVars.MultBonusSpeed],
            });
        }
    }
}


