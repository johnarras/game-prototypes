using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.NPCs.Constants;
using Genrpg.Shared.Vendors.WorldData;
using System.Collections.Generic;

namespace Genrpg.Shared.NPCs.Settings
{
    public class NPCSettings : ParentConstantListSettings<NPCType, NPCTypes>
    {
        public override string Id { get; set; }
    }

    public class NPCType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public long CrafterTypeId { get; set; }
        public long BuildingTypeId { get; set; }
        public List<VendorItem> DefaultVendorItems { get; set; } = new List<VendorItem>();

    }

    public class NPCSettingsDto : ParentSettingsDto<NPCSettings, NPCType>
    {
        public override List<NPCType> Children { get; set; }
        public override NPCSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class NPCSettingsLoader : ParentSettingsLoader<NPCSettings, NPCType> { }

    public class ItemSettingsMapper : ParentSettingsMapper<NPCSettings, NPCType, NPCSettingsDto> { }

}


