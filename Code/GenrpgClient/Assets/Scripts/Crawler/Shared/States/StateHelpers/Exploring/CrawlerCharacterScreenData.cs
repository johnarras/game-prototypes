using MessagePack;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class CrawlerCharacterScreenData
    {
        public CrawlerCharacterScreenData()
        {
        }

        public PartyMember Unit { get; set; }
    }
}


