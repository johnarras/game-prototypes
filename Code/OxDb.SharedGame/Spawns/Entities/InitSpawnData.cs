using OxDb.SharedGame.MapObjects.MapObjectAddons.Entities;
using OxDb.SharedGame.Spawns.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Spawns.Entities
{
    // Used to set up a spawn, not exact same object to allow us to add/remove extra data relative to the final spawn.
    public class InitSpawnData
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public string Name { get; set; }
        public float SpawnX { get; set; }
        public float SpawnZ { get; set; }
        public float Rot { get; set; }
        public long ZoneId { get; set; }
        public string LocationId { get; set; }
        public string LocationPlaceId { get; set; }
        public long FactionTypeId { get; set; } = 1;
        public int ZoneOverridePercent { get; set; }
        public int SpawnSeconds { get; set; } = SpawnConstants.DefaultSpawnSeconds;
        public List<IMapObjectAddon> Addons { get; set; } = new List<IMapObjectAddon>();
    }
}


