using OxDb.RequestServer.Core;
using OxDb.RequestServer.GameClientRequests.RequestHandlers;
using OxDb.RequestServer.Purchasing.Services;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Purchasing.PlayerData;
using OxDb.SharedGame.Purchasing.WebApi.RefreshStores;

namespace OxDb.RequestServer.Purchasing.RequestHandlers
{
    public class RefreshStoresHandler : BaseClientUserRequestHandler<RefreshStoresRequest>
    {
        private IServerPurchasingService _purchasingService = null;

        protected override async Task InnerHandleMessage(WebContext context, RefreshStoresRequest request, CancellationToken token)
        {
            CoreCharacter coreCh = await _repoService.Load<CoreCharacter>(request.CharId);
            Character ch = new Character(coreCh);

            PlayerStoreOfferData offerData = await _purchasingService.GetCurrentStores(context, ch, true, token);

            RefreshStoresResponse response = new RefreshStoresResponse();

            response.Stores = offerData;

            if (response != null)
            {
                context.AddResponse(response);
            }

        }
    }
}


