using Genrpg.Shared.Client.Interfaces;
using Genrpg.Shared.Crawler.Monsters.Entities;

namespace Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents
{
    public class RefreshUnitStatus : IClientEvent
    {
        public CrawlerUnit Unit { get; set; }
        public long ElementTypeId { get; set; }
    }
}


