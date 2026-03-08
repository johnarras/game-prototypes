using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Animals.Services;
using Genrpg.Shared.Trader.Holdings.PlayerData;

namespace Genrpg.RequestServer.Trader.Skins.RewardHelpers
{
    public class SkinTypeAsyncRewardHelper : IAsyncRewardHelper
    {
        private IAnimalService _animalService = null;

        public long HelperKey => EntityTypes.Skin;

        public async Task<long> GetQuantityAsync(WebContext context, long entityId)
        {
            return _animalService.GetSkinQuantity(await context.GetAsync<HoldingsData>(), entityId);    
        }

        public async Task<bool> GiveRewardAsync(WebContext context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            _animalService.AddSkinToHoldings(context.core, await context.GetAsync<HoldingsData>(), entityId);

            return true;
        }
    }
}
