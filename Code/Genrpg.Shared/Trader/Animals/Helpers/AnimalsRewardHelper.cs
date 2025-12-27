using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Trader.Animals.Services;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Trader.Animals.Helpers
{
    public class AnimalsRewardHelper : IRewardHelper
    {
        private IAnimalService _animalService = null;

        public long HelperKey => EntityTypes.Animal;

        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
            _animalService.AddAnimalToHoldings(obj.Get<CoreUserData>(), obj.Get<HoldingsData>(), entityId);
            HoldingsData holdings = obj.Get<HoldingsData>();

            return true;
        }
    }
}


