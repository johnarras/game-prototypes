using Genrpg.Shared.Quests.MapObjectAddons;
using Genrpg.Shared.Serialization.Attributes;
using MessagePack;

namespace Genrpg.Shared.MapObjects.MapObjectAddons.Entities
{
    // Used for addons to a map object
    // Note: For serialization purposes all implementations must do the Union thing here.

    [MessagePackInterface]
    [Union(0 ,typeof(Genrpg.Shared.Vendors.MapObjectAddons.VendorAddon))]
    [Union(1 ,typeof(Genrpg.Shared.Quests.MapObjectAddons.QuestAddon))]
    [Union(2 ,typeof(Genrpg.Shared.MapMods.MapObjectAddons.MapModAddon))]
    public interface IMapObjectAddon
    {
        long GetAddonType();
    }
}


