using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.Purchasing.Constants;


namespace OxDb.SharedGame.Purchasing.WebApi.ValidatePurchase
{
    public class ValidatePurchaseResponse : IWebResponse
    {
        public EPurchaseValidationStates State { get; set; }

        public string ErrorMessage { get; set; }

        public RewardData Rewards { get; set; }
    }
}


