using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Purchasing.Services;
using Genrpg.Shared.Purchasing.WebApi.ValidatePurchase;

namespace Genrpg.RequestServer.Purchasing.RequestHandlers
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


