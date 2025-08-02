using Azure.Core;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Markers.WebApi;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.UserStats.Constants;
using Microsoft.Extensions.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Markers.Services
{
    public interface IMarkerService : IInjectable
    {
        Task SetMarker(WebContext context, long markerId, long markerTier);
    }

    public class MarkerService : IMarkerService
    {
        protected IGameData _gameData = null;
        public async Task SetMarker(WebContext context, long markerId, long markerTier)
        {
            Marker marker = _gameData.Get<MarkerSettings>(context.user).Get(markerId);

            if (marker == null || marker.MaxTier < markerTier)

            {
                context.Responses.AddResponse(new SetMarkerResponse() { Success = false });
                return;
            }
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            userData.Vars.Set(UserVars.MarkerId, markerId);
            userData.Vars.Set(UserVars.MarkerTier, markerTier);

            context.Responses.AddResponse(new SetMarkerResponse() { Success = true, MarkerId = markerId, MarkerTier = markerTier });
        }
    }
}
