using Assets.Scripts.ClientEvents.UI;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;
using Genrpg.Shared.UI.Constants;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Vendors
{
    public class VendorHelper : BuildingStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.Vendor;
        public override long TriggerBuildingId() { return BuildingTypes.Equipment; }
        public override bool HideBigPanels() { return true; }
        protected override bool OnlyUseBGImage() { return true; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData crawlerStateData = CreateStateData();
            crawlerStateData.BGSpriteName = CrawlerClientConstants.VendorImage;
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.CrawlerVendor));

            await Task.CompletedTask;
            return crawlerStateData;
        }
    }
}


