using MessagePack;
using OxDb.SharedGame.MapObjects.MapObjectAddons.Constants;
using OxDb.SharedGame.MapObjects.MapObjectAddons.Entities;
using System.Collections.Generic;

namespace OxDb.SharedGame.MapMods.MapObjectAddons
{
    [MessagePackObject]
    public class MapModAddon : BaseMapObjectAddon
    {
        public override long GetAddonType() { return MapObjectAddonTypes.MapMod; }

        [Key(0)] public List<MapModEffect> Effects { get; set; } = new List<MapModEffect>();

        [Key(1)] public long OwnerEntityTypeId { get; set; }
        [Key(2)] public string OwnerId { get; set; }
        [Key(3)] public float Radius { get; set; }
        [Key(4)] public int TriggerTimes { get; set; }
        [Key(5)] public string Name { get; set; }
    }
}


