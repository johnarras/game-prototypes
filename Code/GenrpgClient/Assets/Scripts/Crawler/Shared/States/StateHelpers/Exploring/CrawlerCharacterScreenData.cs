using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedGame.Crawler.Parties.PlayerData;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Exploring
{
    public class CrawlerCharacterScreenData : IClientEvent
    {
        public CrawlerCharacterScreenData()
        {
        }

        public PartyMember Unit { get; set; }
    }
}


