using Assets.Scripts.Login.Messages.Core;

using System.Threading;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.MapServer.WebApi.LoadIntoMap;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class LoadIntoMapResponseHandler : BaseClientWebResponseHandler<LoadIntoMapResponse>
    {

        public override int Priority() { return 1000; }

        private IZoneGenService _zoneGenService;
        protected override void InnerProcess(LoadIntoMapResponse result, CancellationToken token)
        {
            _zoneGenService.OnLoadIntoMap(result, token);
        }
    }
}
