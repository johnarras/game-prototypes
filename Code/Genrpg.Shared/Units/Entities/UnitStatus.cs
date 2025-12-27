using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.WorldData;
using Genrpg.Shared.MapObjects.MapObjectAddons.Constants;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;

namespace Genrpg.Shared.Units.Entities
{

    public class UnitStatus : BaseWorldData, IId, IStringOwnerId
    {
        public override void Delete(IRepositoryService repoSystem) { repoSystem.Delete(this); }
        public override string Id { get; set; }
        public string OwnerId { get; set; }
        public string ObjId { get; set; }
        public long IdKey { get; set; }
        public string MapId { get; set; }

        public List<IMapObjectAddon> Addons { get; set; } = new List<IMapObjectAddon>();

    }
}


