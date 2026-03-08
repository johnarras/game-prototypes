using Genrpg.Shared.Client.Interfaces;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using MessagePack;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class CrawlerCharacterScreenData : IClientEvent
    {
        public CrawlerCharacterScreenData()
        {
        }

        public PartyMember Unit { get; set; }
    }
}


