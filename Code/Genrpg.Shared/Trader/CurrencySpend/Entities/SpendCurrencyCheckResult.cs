using Genrpg.Shared.Trader.CurrencySpend.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Trader.CurrencySpend.Entities
{
    public enum ESpendCurrencyCheckState
    {
        Success = 0,
        GenericFailure = 1,
        NotEnoughCurrency =2,
        LocationDoesNotExist = 3,
        SpendTypeDoesNotExist= 4,
        LocationIsNotValid = 5,
        CurrencyTypeIsIncorrect=6,
        CurrencyQuantityIsIncorrect=7,
        NoSpendRewards = 8, 
        SpendQuantityMustBePositive=9,
        FailedToGiveRewards=10,
    }


    public class SpendCurrencyCheckResult
    {
        public ESpendCurrencyCheckState State { get; set; } = ESpendCurrencyCheckState.GenericFailure;
        public FullSpendLocation FullLocation { get; set; }
        public SpendType SpendType { get; set; }
    }
}
