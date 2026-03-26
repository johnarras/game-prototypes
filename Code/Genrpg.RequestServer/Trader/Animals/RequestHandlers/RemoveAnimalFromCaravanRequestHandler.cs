using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Trader.CaravanMembers.WebApi;
using Genrpg.Shared.Trader.Caravans.Services;

namespace Genrpg.RequestServer.Trader.CaravanMembers.RequestHandlers
{
    public class RemoveCaravanMemberFromCaravanRequestHandler : BaseClientUserRequestHandler<RemoveCaravanMemberFromCaravanRequest>
    {
        protected ICaravanService _caravanService = null;
        protected override async Task InnerHandleMessage(WebContext context, RemoveCaravanMemberFromCaravanRequest request, CancellationToken token)
        {

            context.AddResponse(await _caravanService.RemoveMemberFromCaravan(context, request.CaravanMemberId, false));

        }
    }
}
