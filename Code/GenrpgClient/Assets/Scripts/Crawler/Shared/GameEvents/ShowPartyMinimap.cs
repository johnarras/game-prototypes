using Genrpg.Shared.Client.Interfaces;
using Genrpg.Shared.Crawler.Parties.PlayerData;

namespace Genrpg.Shared.Crawler.GameEvents
{
    public class ShowPartyMinimap : IClientEvent
    {
        public PartyData Party { get; set; }
        public bool PartyArrowOnly { get; set; }
    }
}


