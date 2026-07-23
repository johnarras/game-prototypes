using OxDb.Client.ClientEvents.UI;
using OxDb.SharedGame.Buildings.Constants;
using OxDb.SharedGame.Crawler.Constants;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers.Buildings;
using OxDb.SharedGame.UI.Constants;
using System.Threading;
using System.Threading.Tasks;


namespace OxDb.SharedGame.Crawler.States.StateHelpers.Vendors
{
    public class VendorHelper : BuildingStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.Vendor;
        public override long TriggerBuildingId() { return BuildingTypes.Equipment; }
        public override bool HideBigPanels() { return true; }
        protected override bool OnlyUseBGImage() { return true; }

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData crawlerStateData = CreateStateData();
            crawlerStateData.BGSpriteName = CrawlerClientConstants.BuildingImage;
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.CrawlerVendor));

            await Task.CompletedTask;
            return crawlerStateData;
        }
    }
}


