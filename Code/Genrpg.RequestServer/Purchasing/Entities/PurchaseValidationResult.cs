using Genrpg.Shared.Purchasing.Constants;

namespace Genrpg.RequestServer.Purchasing.Entities
{
    public class PurchaseValidationResult
    {
        public EPurchaseValidationStates State { get; set; }

        public string ErrorMessage { get; set; }

    }
}


