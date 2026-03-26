using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.MapServer.WebApi.UploadMap;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Login.MessageHandlers.Core
{
    public class UploadMapResultHandler : BaseClientWebResponseHandler<UploadMapResponse>
    {
        protected override async Awaitable InnerProcess(UploadMapResponse result, CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}


