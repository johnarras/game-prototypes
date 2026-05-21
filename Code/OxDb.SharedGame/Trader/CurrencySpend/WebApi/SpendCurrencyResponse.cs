using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.Trader.CurrencySpend.Entities;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.CurrencySpend.WebApi
{
    public class SpendCurrencyResponse : IWebResponse
    {

        public ESpendCurrencyCheckState State { get; set; }

        public string Message { get; set; }

        public List<RewardList> Rewards { get; set; } = new List<RewardList>();

        public string ExtraRewardArgs { get; set; } = string.Empty;
    }
}
