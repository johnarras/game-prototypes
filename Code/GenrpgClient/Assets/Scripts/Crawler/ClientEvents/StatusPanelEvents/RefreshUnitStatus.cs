using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedGame.Crawler.Monsters.Entities;

namespace Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents
{
    public class RefreshUnitStatus : IClientEvent
    {
        public CrawlerUnit Unit { get; set; }
        public long ElementTypeId { get; set; }
    }
}


