using OxDb.SharedGame.Purchasing.Constants;

namespace OxDb.RequestServer.Purchasing.Entities
{
    public class PurchaseValidationResult
    {
        public EPurchaseValidationStates State { get; set; }

        public string ErrorMessage { get; set; }

        public int Status { get; set; }

    }
}


