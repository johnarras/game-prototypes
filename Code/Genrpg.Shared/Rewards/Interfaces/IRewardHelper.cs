
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using System.Threading.Tasks;

namespace Genrpg.Shared.Rewards.Interfaces
{
    /// <summary>
    /// Only use this inside of the website since it has to do async loads.
    /// </summary>
    public interface IRewardHelper : ISetupDictionaryItem<long>
    {
        /// <summary>
        /// Website async only.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="ch"></param>
        /// <param name="entityId"></param>
        /// <param name="quantity"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        Task<bool> GiveReward(IUnitDataLookup lookup, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp);

        Task<long> GetQuantity(IUnitDataLookup lookup, long entityId);
    }
}


