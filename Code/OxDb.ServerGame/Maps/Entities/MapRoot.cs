using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.DataStores.Categories.WorldData;
using OxDb.SharedGame.MapServer.Entities;

namespace OxDb.ServerGame.Maps.Entities
{
    public class MapRoot : BaseWorldData, IMapRoot
    {
        public override string Id { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }

        public int BlockCount { get; set; }
        public float ZoneSize { get; set; }

        public long Seed { get; set; }

        public int MapVersion { get; set; }

        public int SpawnX { get; set; }
        public int SpawnY { get; set; }

        public long OverrideZoneId { get; set; }
        public float OverrideZonePercent { get; set; }

        public override void Delete(IRepositoryService repoSystem) { repoSystem.Delete(this); }

    }
}


