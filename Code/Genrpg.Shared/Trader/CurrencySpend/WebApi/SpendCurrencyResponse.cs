using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Website.Interfaces;
using MessagePack.Resolvers;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.CurrencySpend.WebApi
{
    public class SpendCurrencyResponse : IWebResponse
    {

        public ESpendCurrencyCheckState State { get; set; }

        public string Message { get; set; }
        public List<Reward> Rewards { get; set; } = new List<Reward>();

        public string ExtraRewardArgs { get; set; } = string.Empty;
    }
}
