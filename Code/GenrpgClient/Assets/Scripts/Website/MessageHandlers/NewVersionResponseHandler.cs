using OxDb.Client.Login.Messages.Core;
using OxDb.SharedGame.GameAuth.WebApi.NewVersions;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Website.MessageHandlers
{
    public class NewVersionResponseHandler : BaseClientWebResponseHandler<NewVersionResponse>
    {
        protected override async ValueTask InnerProcess(NewVersionResponse response, CancellationToken token)
        {
            _dispatcher.Dispatch(response);
            await Task.CompletedTask;
        }
    }
}


