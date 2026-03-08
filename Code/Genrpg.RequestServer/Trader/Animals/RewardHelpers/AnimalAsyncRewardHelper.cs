using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Animals.Services;
using Genrpg.Shared.Trader.Holdings.PlayerData;

namespace Genrpg.RequestServer.Trader.Animals.RewardHelpers
{
    public class AnimalAsyncRewardHelper : IAsyncRewardHelper
    {
        private IAnimalService _animalService = null;

        public long HelperKey => EntityTypes.Animal;

        public async Task<bool> GiveRewardAsync(WebContext context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            _animalService.AddAnimalToHoldings(context.core, await context.GetAsync<HoldingsData>(), entityId);
            return true;
        }

        public async Task<long> GetQuantityAsync(WebContext context, long entityId)
        {
            return _animalService.GetAnimalQuantity(await context.GetAsync<HoldingsData>(), entityId);
        }
    }
}


