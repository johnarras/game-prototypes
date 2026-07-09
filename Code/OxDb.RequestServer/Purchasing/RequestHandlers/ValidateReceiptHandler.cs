using OxDb.RequestServer.Core;
using OxDb.RequestServer.GameClientRequests.RequestHandlers;
using OxDb.RequestServer.Purchasing.Services;
using OxDb.SharedGame.Purchasing.WebApi.ValidatePurchase;

namespace OxDb.RequestServer.Purchasing.RequestHandlers
{
    public class ValidatePurchaseHandler : BaseClientUserRequestHandler<ValidatePurchaseRequest>
    {

        private IServerPurchasingService _purchasingService = null;
        protected override async Task InnerHandleMessage(WebContext context, ValidatePurchaseRequest request, CancellationToken token)
        {
            await _purchasingService.ValidatePurchase(context, request, token);
        }
    }
}


