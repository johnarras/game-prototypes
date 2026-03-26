using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using System.Collections.Generic;

namespace Genrpg.Shared.Inventory.Settings.ItemTypes
{
    public class ItemTypeSettings : ParentSettings<ItemType>
    {
        public int GenSameStatPercent { get; set; }
        public int GenSameStatBonusPct { get; set; }
        public int GenGlobalScalingPercent { get; set; }
        public override string Id { get; set; }

    }

    public class ItemTypeSettingsDto : ParentSettingsDto<ItemTypeSettings, ItemType>
    {
        public override List<ItemType> Children { get; set; }
        public override ItemTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class ItemTypeSettingsLoader : ParentSettingsLoader<ItemTypeSettings, ItemType> { }

    public class ItemSettingsMapper : ParentSettingsMapper<ItemTypeSettings, ItemType, ItemTypeSettingsDto> { }


    public class ItemHelper : BaseEntityHelper<ItemTypeSettings, ItemType>
    {
        public override long HelperKey => EntityTypes.Item;
    }
}


