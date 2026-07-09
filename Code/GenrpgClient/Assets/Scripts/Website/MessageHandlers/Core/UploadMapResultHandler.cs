using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedGame.MapServer.WebApi.UploadMap;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Login.MessageHandlers.Core
{
    public class UploadMapResultHandler : BaseClientWebResponseHandler<UploadMapResponse>
    {
        protected override async ValueTask InnerProcess(UploadMapResponse result, CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}


