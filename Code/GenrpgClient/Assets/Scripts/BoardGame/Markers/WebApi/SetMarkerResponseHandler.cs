using Assets.Scripts.BoardGame.Markers.Services;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.BoardGame.Markers.WebApi;
using Genrpg.Shared.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.BoardGame.Markers.WebApi
{
    public class SetMarkerResponseHandler : BaseClientWebResponseHandler<SetMarkerResponse>
    {
        protected IClientMarkerService _clientMarkerService = null;
        protected override void InnerProcess(SetMarkerResponse response, CancellationToken token)
        {
            if (response.Success)
            {
                _clientMarkerService.ClientSetMarkerId(response.MarkerId, response.MarkerTier);
            }
        }
    }
}
