using OxDb.SharedGame.Trader.CurrencySpend.Settings;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.CurrencySpend.Entities
{
    public class FullSpendLocation
    {
        public bool IsValid { get; set; }
        public SpendLocation Location { get; set; }
        public List<SpendType> SpendTypes { get; set; } = new List<SpendType>();
    }
}
