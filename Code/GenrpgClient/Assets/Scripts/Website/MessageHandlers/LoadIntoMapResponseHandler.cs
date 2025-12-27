using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.MapServer.WebApi.LoadIntoMap;
using System.Threading;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class LoadIntoMapResponseHandler : BaseClientWebResponseHandler<LoadIntoMapResponse>
    {

        public override int Priority() { return 1000; }

        private IZoneGenService _zoneGenService = null;
        protected override void InnerProcess(LoadIntoMapResponse result, CancellationToken token)
        {
            _zoneGenService.OnLoadIntoMap(result, token);
        }
    }
}


