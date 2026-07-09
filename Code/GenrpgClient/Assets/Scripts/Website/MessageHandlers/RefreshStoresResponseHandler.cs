using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedGame.Purchasing.WebApi.RefreshStores;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class RefreshStoresResponseHandler : BaseClientWebResponseHandler<RefreshStoresResponse>
    {
        protected override async ValueTask InnerProcess(RefreshStoresResponse result, CancellationToken token)
        {
            _gs.ch.Set(result.Stores);
            _dispatcher.Dispatch(result);
            await Task.CompletedTask;
        }
    }
}


