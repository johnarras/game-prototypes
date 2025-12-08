using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Trader.Animals.Helpers
{
    public class AnimalsRewardHelper : IRewardHelper
    {
        public long HelperKey => EntityTypes.Animal;

        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
            HoldingsData holdings = obj.Get<HoldingsData>();

            holdings.AnimalsOwned.SetBit(entityId);

            return true;
        }
    }
}
