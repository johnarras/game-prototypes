

using Genrpg.Shared.Trader.CurrencySpend.Constants;

namespace Genrpg.Shared.Trader.CurrencySpend.SpendHelpers
{
    public class RepairSpendLocationHelper : TempleSpendLocationHelper
    {
        public override long HelperKey => SpendLocations.Repair;

        protected override string FixString => "Repair";
    }
}
