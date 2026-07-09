using OxDb.RequestServer.Core;
using OxDb.RequestServer.GameClientRequests.RequestHandlers;
using OxDb.RequestServer.Purchasing.Services;
using OxDb.SharedGame.Purchasing.WebApi.InitializePurchase;

namespace OxDb.RequestServer.Purchasing.RequestHandlers
{
    public class InitiatePurchaseHandler : BaseClientUserRequestHandler<InitiatePurchaseRequest>
    {

        private IServerPurchasingService _purchasingService = null;
        protected override async Task InnerHandleMessage(WebContext context, InitiatePurchaseRequest request, CancellationToken token)
        {
            await _purchasingService.InitiatePurchase(context, request, token);
        }
    }
}


