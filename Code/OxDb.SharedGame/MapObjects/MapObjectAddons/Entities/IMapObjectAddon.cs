using MessagePack;
using OxDb.SharedCore.Serialization.Attributes;

namespace OxDb.SharedGame.MapObjects.MapObjectAddons.Entities
{
    // Used for addons to a map object
    // Note: For serialization purposes all implementations must do the Union thing here.

    [MessagePackInterface]
    [Union(0 ,typeof(OxDb.SharedGame.Vendors.MapObjectAddons.VendorAddon))]
    [Union(1 ,typeof(OxDb.SharedGame.Quests.MapObjectAddons.QuestAddon))]
    [Union(2 ,typeof(OxDb.SharedGame.MapMods.MapObjectAddons.MapModAddon))]
    public interface IMapObjectAddon
    {
        long GetAddonType();
    }
}


