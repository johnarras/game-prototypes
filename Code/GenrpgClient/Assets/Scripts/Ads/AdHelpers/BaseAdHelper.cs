using OxDb.Client.FloatingText.ClientEvents;
using OxDb.Client.Networking.Services;
using UnityEngine;

namespace OxDb.Client.Ads.AdHelpers
{
    public abstract class BaseAdHelper : IAdHelper
    {

        protected IDispatcher _dispatcher = null;
        protected IClientWebRequestService _webService = null;

        public abstract EAdTypes HelperKey { get; }

        protected abstract Awaitable<AdResult> ShowSpecificAd(AdArgs args = null);

        public async Awaitable<AdResult> ShowAd(AdArgs args = null)
        {
            AdResult result = await ShowSpecificAd(args);


            if (!result.Success)
            {
                _dispatcher.Dispatch(new ShowFloatingText(result.ErrorMessage, EFloatingTextArt.Error));
                return result;
            }

            // Send to server.


            return result;
        }
    }
}
