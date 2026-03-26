

using Genrpg.Shared.Entities.Constants;

namespace Genrpg.Shared.Attributes.Helpers
{

    // These are all in the same file since they are so closely linked, I want to be
    // able to inspect them all or change them all at once.


    /// <summary>
    /// Set current CoreCurrency quantity.
    /// </summary>
    public class BaseGameplayStatRewardHelper : BaseAttributeRewardHelper
    {
        public override long HelperKey => EntityTypes.BaseGameplayStat;
    }

    public class BonusGameplayStatRewardHelper : BaseAttributeRewardHelper
    {
        public override long HelperKey => EntityTypes.BonusGameplayStat;
    }

    public class BaseCurrencyRegenRewardHelper : BaseAttributeRewardHelper
    {
        public override long HelperKey => EntityTypes.BaseCurrencyRegen;
    }

    public class BonusCurrencyRegenRewardHelper : BaseAttributeRewardHelper
    {
        public override long HelperKey => EntityTypes.BonusCurrencyRegen;
    }

    public class BaseCurrencyStorageRewardHelper : BaseAttributeRewardHelper
    {
        public override long HelperKey => EntityTypes.BaseCurrencyStorage;
    }

    public class BonusCurrencyStorageRewardHelper : BaseAttributeRewardHelper
    {
        public override long HelperKey => EntityTypes.BonusCurrencyStorage;
    }

    public class BaseTravelDayCurrencyRewardHelper : BaseAttributeRewardHelper
    {
        public override long HelperKey => EntityTypes.BaseTravelDayCurrency;
    }

    public class BonusTravelDayCurrencyRewardHelper : BaseAttributeRewardHelper
    {
        public override long HelperKey => EntityTypes.BonusTravelDayCurrency;
    }
}


