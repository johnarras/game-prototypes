using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Purchasing.Services;
using Genrpg.Shared.Purchasing.WebApi.InitializePurchase;

namespace Genrpg.RequestServer.Purchasing.RequestHandlers
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
