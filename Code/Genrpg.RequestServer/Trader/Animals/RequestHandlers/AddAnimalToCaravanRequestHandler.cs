using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Trader.CaravanMembers.WebApi;
using Genrpg.Shared.Trader.Caravans.Services;

namespace Genrpg.RequestServer.Trader.CaravanMembers.RequestHandlers
{
    public class AddCaravanMemberToCaravanRequestHandler : BaseClientUserRequestHandler<AddCaravanMemberToCaravanRequest>
    {
        protected ICaravanService _caravanService = null;
        protected override async Task InnerHandleMessage(WebContext context, AddCaravanMemberToCaravanRequest request, CancellationToken token)
        {

            context.AddResponse(await _caravanService.AddMemberToCaravan(context, request.CaravanMemberId, false));

        }
    }
}
