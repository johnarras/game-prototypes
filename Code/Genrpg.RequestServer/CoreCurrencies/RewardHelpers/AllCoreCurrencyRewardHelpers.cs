using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.RewardHelpers.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.CoreCurrencies.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.RequestServer.CoreCurrencies.RewardHelpers
{

    // These are all in the same file since they are so closely linked, I want to be
    // able to inspect them all or change them all at once.


    /// <summary>
    /// Set current CoreCurrency quantity.
    /// </summary>
    public abstract class BaseCoreCurrencyRewardHelper : BaseAsyncRewardHelper
    {
        protected abstract void InnerGiveReward(CoreCurrencyStatus status, long quantity);
        public override async Task GiveRewardsAsync(WebContext context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            InnerGiveReward((await context.GetAsync<CoreUserData>()).Currencies.GetStatus(entityId), quantity);
        }
    }

    public abstract class AddCoreCurrencyRewardHelper : BaseCoreCurrencyRewardHelper
    {
        protected abstract long DataIndex { get; }
        protected override void InnerGiveReward(CoreCurrencyStatus status, long quantity)
        {
            status.Add(DataIndex, quantity);
        }
    }

    public abstract class SetCoreCurrencyRewardHelper : BaseCoreCurrencyRewardHelper
    {
        protected abstract long DataIndex { get; }
        protected override void InnerGiveReward(CoreCurrencyStatus status, long quantity)
        {
            status.Set(DataIndex, quantity);
        }
    }

    // Actual implementations for the 5 systems.
    public class CoreCurrencyRewardHelper : AddCoreCurrencyRewardHelper
    {
        public override long Key => EntityTypes.CoreCurrency;
        protected override long DataIndex => CurrencyDataOffset.Curr;
    }

    public class BaseRegenCoreCurrencyRewardHelper : SetCoreCurrencyRewardHelper
    {
        public override long Key => EntityTypes.BaseCoreCurrencyRegen;
        protected override long DataIndex => CurrencyDataOffset.BaseRegen;
    }

    public class BaseStorageCoreCurrencyRewardHelper : SetCoreCurrencyRewardHelper
    {
        public override long Key => EntityTypes.BaseCoreCurrencyStorage;
        protected override long DataIndex => CurrencyDataOffset.BaseStorage;
    }

    public class BonusRegenCurrencyRewardHelper : AddCoreCurrencyRewardHelper
    {
        public override long Key => EntityTypes.BonusCoreCurrencyRegen;
        protected override long DataIndex => CurrencyDataOffset.BonusRegen;
    }

    public class BonusStorageCurrencyRewardHelper : AddCoreCurrencyRewardHelper
    {
        public override long Key => EntityTypes.BonusCoreCurrencyStorage;
        protected override long DataIndex => CurrencyDataOffset.BonusStorage;
    }
}
