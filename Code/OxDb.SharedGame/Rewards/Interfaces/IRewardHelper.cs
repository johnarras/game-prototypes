
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;

using System.Threading.Tasks;

namespace OxDb.SharedGame.Rewards.Interfaces
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


