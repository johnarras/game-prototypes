
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Interfaces;
using Genrpg.Shared.Rewards.Services;
using System.Threading.Tasks;

namespace Genrpg.Shared.Rewards.RewardHelpers.Core
{
    public abstract class BaseRewardHelper : IRewardHelper
    {
        protected IRepositoryService _repoService = null;
        protected IGameData _gameData = null;
        protected IRewardService _serverRewardService = null;

        public abstract long HelperKey { get; }

        public abstract Task<long> GetQuantity(IUnitDataLookup context, long entityId);

        public abstract Task<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, RewardParams rp);
    }
}


