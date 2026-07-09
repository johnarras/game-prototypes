using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedGame.MapServer.WebApi.LoadIntoMap;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class LoadIntoMapResponseHandler : BaseClientWebResponseHandler<LoadIntoMapResponse>
    {

        public override int Priority() { return 1000; }

        private IZoneGenService _zoneGenService = null;
        protected override async ValueTask InnerProcess(LoadIntoMapResponse result, CancellationToken token)
        {
            await _zoneGenService.OnLoadIntoMap(result, token);
        }
    }
}


