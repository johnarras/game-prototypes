using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.CoreCurrencies.RewardHelpers
{
    public class CoreCurrencyRewardHelper : IRewardHelper
    {
        public long HelperKey => EntityTypes.CoreCurrency;

        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
            CoreData coreData = obj.Get<CoreData>();

            coreData.Currencies.Add(entityId, quantity);

            return true;
        }
    }
}


