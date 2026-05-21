

using OxDb.SharedGame.Trader.CurrencySpend.Constants;

namespace OxDb.SharedGame.Trader.CurrencySpend.SpendHelpers
{
    public class RepairSpendLocationHelper : TempleSpendLocationHelper
    {
        public override long HelperKey => SpendLocations.Repair;

        protected override string FixString => "Repair";
    }
}
