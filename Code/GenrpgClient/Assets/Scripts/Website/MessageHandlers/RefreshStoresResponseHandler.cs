using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Purchasing.WebApi.RefreshStores;
using System.Threading;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class RefreshStoresResponseHandler : BaseClientWebResponseHandler<RefreshStoresResponse>
    {
        protected override void InnerProcess(RefreshStoresResponse result, CancellationToken token)
        {
            _gs.ch.Set(result.Stores);
            _dispatcher.Dispatch(result);
        }
    }
}
