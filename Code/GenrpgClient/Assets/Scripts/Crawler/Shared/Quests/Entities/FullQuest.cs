using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.Worlds.Entities;

namespace Genrpg.Shared.Crawler.Quests.Entities
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
