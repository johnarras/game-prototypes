using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Vendors.WorldData;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.NPCs.Constants;

namespace Genrpg.Shared.NPCs.Settings
{
    [MessagePackObject]
    public class NPCSettings : ParentConstantListSettings<NPCType,NPCTypes>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class NPCType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public long CrafterTypeId { get; set; }
        [Key(9)] public long BuildingTypeId { get; set; }
        [Key(10)] public List<VendorItem> DefaultVendorItems { get; set; } = new List<VendorItem>();

    }

    public class NPCSettingsDto : ParentSettingsDto<NPCSettings, NPCType> { }

    public class NPCSettingsLoader : ParentSettingsLoader<NPCSettings, NPCType> { }

    public class ItemSettingsMapper : ParentSettingsMapper<NPCSettings, NPCType, NPCSettingsDto> { }

}
