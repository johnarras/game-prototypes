using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.DataStores.Categories.WorldData;
using OxDb.SharedGame.Interfaces;
using OxDb.SharedGame.MapObjects.MapObjectAddons.Entities;
using System.Collections.Generic;

namespace OxDb.SharedGame.Units.Entities
{

    public class UnitStatus : BaseWorldData, IId, IStringOwnerId
    {
        public override void Delete(IRepositoryService repoSystem) { repoSystem.Delete(this); }
        public override string Id { get; set; }
        public override string Name { get; set; }
        public string OwnerId { get; set; }
        public string ObjId { get; set; }
        public long IdKey { get; set; }
        public string MapId { get; set; }

        public List<IMapObjectAddon> Addons { get; set; } = new List<IMapObjectAddon>();

    }
}


