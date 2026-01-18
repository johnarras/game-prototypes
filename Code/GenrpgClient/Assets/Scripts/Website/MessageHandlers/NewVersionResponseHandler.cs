using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.GameAuth.WebApi.NewVersions;
using System.Threading;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class NewVersionResponseHandler : BaseClientWebResponseHandler<NewVersionResponse>
    {
        protected override void InnerProcess(NewVersionResponse response, CancellationToken token)
        {
            _dispatcher.Dispatch(response);
        }
    }
}


