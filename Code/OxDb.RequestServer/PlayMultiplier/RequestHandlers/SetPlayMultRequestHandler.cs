using OxDb.RequestServer.Core;
using OxDb.RequestServer.GameClientRequests.RequestHandlers;
using OxDb.RequestServer.PlayMultiplier.Services;
using OxDb.SharedGame.PlayMultiplier.WebApi;

namespace OxDb.RequestServer.PlayMultiplier.RequestHandlers
{
    public class SetPlayMultRequestHandler : BaseClientUserRequestHandler<SetPlayMultRequest>
    {
        IServerPlayMultService _playMultService = null;
        protected override async Task InnerHandleMessage(WebContext context, SetPlayMultRequest request, CancellationToken token)
        {
            await _playMultService.SetPlayMult(context, request.PlayMult);
        }
    }
}


