
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Rewards.Interfaces;
using OxDb.SharedGame.Rewards.Services;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Rewards.RewardHelpers.Core
{
    public abstract class BaseRewardHelper : IRewardHelper
    {
        protected IRepositoryService _repoService = null;
        protected IGameData _gameData = null;
        protected IRewardService _serverRewardService = null;

        public abstract long HelperKey { get; }

        public abstract ValueTask<long> GetQuantity(IUnitDataLookup context, long entityId);

        public abstract ValueTask<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp);
    }
}


