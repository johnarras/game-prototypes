using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.GameAuth.WebApi.NewVersions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class NewVersionResponseHandler : BaseClientWebResponseHandler<NewVersionResponse>
    {
        protected override async Awaitable InnerProcess(NewVersionResponse response, CancellationToken token)
        {
            _dispatcher.Dispatch(response);
            await Task.CompletedTask;
        }
    }
}


