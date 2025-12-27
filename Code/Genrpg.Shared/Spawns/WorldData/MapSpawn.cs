using Genrpg.Shared.DataStores.Categories.WorldData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Spawns.WorldData
{
    public class MapSpawn : BaseWorldData, IMapSpawn, IMapOwnerId
    {
        public override string Id { get; set; }
        public string ObjId { get; set; }
        public string OwnerId { get; set; }
        public string MapId { get; set; }
        public DateTime LastSpawnTime { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public string Name { get; set; }
        public float X { get; set; }
        public float Z { get; set; }
        public short Rot { get; set; }
        public long ZoneId { get; set; }
        public string LocationId { get; set; }
        public string LocationPlaceId { get; set; }
        public int SpawnSeconds { get; set; }
        public int OverrideZonePercent { get; set; }
        public long FactionTypeId { get; set; }
        public string AddonString { get; set; } // TODO: better system than this hack
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<IMapObjectAddon> Addons { get; set; } = new List<IMapObjectAddon>();
        public List<IMapObjectAddon> GetAddons()
        {
            if (Addons == null)
            {
                Addons = new List<IMapObjectAddon>();
            }

            if (Addons.Count > 0)
            {
                return Addons;
            }
            return Addons;
        }

        public long GetAddonBits()
        {
            long addonBits = 0;
            List<IMapObjectAddon> addons = GetAddons();

            foreach (IMapObjectAddon addon in addons)
            {
                addonBits |= (long)(1 << (int)addon.GetAddonType());
            }
            return addonBits;
        }
        public override void Delete(IRepositoryService repoSystem) { repoSystem.Delete(this); }
    }
}


