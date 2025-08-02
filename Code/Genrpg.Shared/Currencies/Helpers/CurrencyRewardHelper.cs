using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Helpers;
using Genrpg.Shared.Currencies.PlayerData;

namespace Genrpg.Shared.Currencies.Helpers
{
    public class CurrencyRewardHelper : BaseQuantityRewardHelper<CurrencyData, CurrencyStatus>
    {
        public override long Key => EntityTypes.Currency;
    }
}
