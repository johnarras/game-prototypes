using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.CurrencySpend.WebApi
{
    /// <summary>
    /// Request to spend in-game currency. 
    /// </summary>
    public class SpendCurrencyRequest : IClientUserRequest
    {
        public long SpendLocationId { get; set; }
        public long SpendTypeIndex { get; set; }
        public long SpendCoreCurrencyTypeId { get; set; }
        public long SpendQuantity { get; set; }
        public bool UseCurrentCity { get; set; }
        public long TargetEntityId { get; set; }
        public string ExtraRewardArgs { get; set; }
    }
}
