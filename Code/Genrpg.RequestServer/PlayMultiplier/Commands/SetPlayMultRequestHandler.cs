using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayMultiplier.Services;
using Genrpg.Shared.PlayMultiplier.WebApi;

namespace Genrpg.RequestServer.PlayMultiplier.Commands
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


