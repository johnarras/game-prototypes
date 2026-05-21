using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedGame.Crawler.Parties.PlayerData;

namespace OxDb.SharedGame.Crawler.GameEvents
{
    public class ShowPartyMinimap : IClientEvent
    {
        public PartyData Party { get; set; }
        public bool PartyArrowOnly { get; set; }
    }
}


