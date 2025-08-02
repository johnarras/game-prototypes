using Genrpg.RequestServer.BoardGame.Markers.Services;
using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.RequestHandlers;
using Genrpg.Shared.BoardGame.Markers.WebApi;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Users.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Markers.RequestHandlers
{
    public class SetMarkerRequestHandler : BaseClientUserRequestHandler<SetMarkerRequest>
    {
        protected IMarkerService _markerService = null;
        protected override async Task InnerHandleMessage(WebContext context, SetMarkerRequest request, CancellationToken token)
        {

            await _markerService.SetMarker(context, request.MarkerId, request.MarkerTier);
            
        }
    }
}
