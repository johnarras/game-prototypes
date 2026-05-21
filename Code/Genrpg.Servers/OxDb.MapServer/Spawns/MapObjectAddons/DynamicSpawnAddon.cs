using OxDb.SharedGame.MapObjects.MapObjectAddons.Constants;
using OxDb.SharedGame.MapObjects.MapObjectAddons.Entities;

namespace OxDb.MapServer.Spawns.MapObjectAddons
{
    public class DynamicSpawnAddon : BaseMapObjectAddon
    {
        public override long GetAddonType() { return MapObjectAddonTypes.DynamicSpawn; }

        public string ParentId { get; set; }
    }
}


