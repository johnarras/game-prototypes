using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Crawler.Worlds.Entities
{
    public class WorldQuestItem : IIdName
    {
        public long IdKey { get; set; }
        public string Name { get; set; }
        public long FoundInMapId { get; set; }
        public long UnlocksMapId { get; set; }
        public string GuardName { get; set; }
        public long GuardUnitTypeId { get; set; }
    }
}


