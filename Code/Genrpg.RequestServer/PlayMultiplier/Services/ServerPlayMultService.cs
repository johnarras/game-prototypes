using Genrpg.RequestServer.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.MobileGame.Constants;
using Genrpg.Shared.PlayMultiplier.Services;
using Genrpg.Shared.PlayMultiplier.Settings;
using Genrpg.Shared.PlayMultiplier.WebApi;

namespace Genrpg.RequestServer.PlayMultiplier.Services
{
    public class ServerPlayMultService : IServerPlayMultService
    {
        private ISharedPlayMultService _sharedPlayMultService = null;
        public async Task SetPlayMult(WebContext context, long newPlayMult)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            long level = userData.Level;

            if (userData.Level < 1)
            {
                userData.Level = 1;
            }

            long food = userData.Currencies.Get(CoreCurrencyTypes.Food);

            List<PlayMult> validMults = _sharedPlayMultService.GetValidMults(context.user, level, food);

            bool isOkMult = validMults.Any(x => x.Mult == newPlayMult);

            if (isOkMult == true)
            {
                userData.Mult = newPlayMult;
                context.AddResponse(new SetPlayMultResponse() { Success = true, NewPlayMult = newPlayMult });
            }

            PlayMult okMult = validMults.LastOrDefault(x => x.Mult < newPlayMult);

            context.AddResponse(new SetPlayMultResponse() { Success = false, NewPlayMult = okMult?.Mult ?? MobileGameConstants.MinPlayMult });

        }
    }
}


