using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Crawler.Modes.Services
{
    public interface ICrawlerModeService : IInjectable
    {
        bool StartWithPremadeParty(ECrawlerModes crawlerMode);
        bool GenerateAllMapsAtOnce(ECrawlerModes crawlerMode);
        bool SingleCityMode(ECrawlerModes crawlerMode);
        bool SinglePartyMember(ECrawlerModes crawlerMode);
    }


    public class CrawlerModeService : ICrawlerModeService
    {
        public bool GenerateAllMapsAtOnce(ECrawlerModes crawlerMode)
        {
            return crawlerMode == ECrawlerModes.Crawler;
        }

        public bool SingleCityMode(ECrawlerModes crawlerMode)
        {
            return crawlerMode != ECrawlerModes.Crawler;
        }

        public bool SinglePartyMember(ECrawlerModes crawlerMode)
        {
            return crawlerMode != ECrawlerModes.Crawler;
        }

        public bool StartWithPremadeParty(ECrawlerModes crawlerMode)
        {
            return crawlerMode == ECrawlerModes.Crawler;
        }

    }
}
