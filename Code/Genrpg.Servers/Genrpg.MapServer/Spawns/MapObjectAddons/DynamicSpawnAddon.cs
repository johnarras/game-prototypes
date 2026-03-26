using Genrpg.Shared.MapObjects.MapObjectAddons.Constants;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;

namespace Genrpg.MapServer.Spawns.MapObjectAddons
{
    public class DynamicSpawnAddon : BaseMapObjectAddon
    {
        public override long GetAddonType() { return MapObjectAddonTypes.DynamicSpawn; }

        public string ParentId { get; set; }
    }
}


