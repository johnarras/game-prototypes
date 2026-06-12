using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using System.Collections.Generic;

namespace OxDb.SharedGame.Inventory.Settings.ItemTypes
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
        public override string GetIconSuffix() { return "_001"; }
        public override long HelperKey => EntityTypes.Item;
    }
}


