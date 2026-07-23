using OxDb.Client.Login.Messages.Core;
using OxDb.SharedGame.MapServer.WebApi.UploadMap;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Login.MessageHandlers.Core
{
    public class UploadMapResultHandler : BaseClientWebResponseHandler<UploadMapResponse>
    {
        protected override async ValueTask InnerProcess(UploadMapResponse result, CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}


