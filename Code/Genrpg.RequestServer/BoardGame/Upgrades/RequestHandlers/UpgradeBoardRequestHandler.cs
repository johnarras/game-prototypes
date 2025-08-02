using Genrpg.RequestServer.BoardGame.Upgrades.Services;
using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Upgrades.WebApi;

namespace Genrpg.RequestServer.BoardGame.Upgrades.RequestHandlers
{
    public class UpgradeBoardRequestHandler : BaseClientUserRequestHandler<UpgradeBoardRequest>
    {
        private IServerUpgradeBoardService _ugpradeService = null;
        protected override async Task InnerHandleMessage(WebContext context, UpgradeBoardRequest request, CancellationToken token)
        {
            await _ugpradeService.UpgradeTileType(context, request.TileTypeId);
        }
    }
}
