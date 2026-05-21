using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.Worlds.Entities;

namespace OxDb.SharedGame.Crawler.Quests.Entities
{
    public class FullQuest
    {
        public MapCellDetail NpcDetail { get; set; }
        public CrawlerQuest Quest { get; set; }
        public PartyQuest Progress { get; set; }
        public ECrawlerStates ReturnState { get; set; }

        public bool IsComplete()
        {
            return Quest != null && Progress != null && Progress.CurrQuantity >= Quest.Quantity;
        }
    }
}


