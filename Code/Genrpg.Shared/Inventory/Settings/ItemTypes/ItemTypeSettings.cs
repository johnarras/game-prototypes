using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Inventory.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Inventory.Settings.ItemTypes
{
    public class ItemTypeSettings : ParentSettings<ItemType>
    {
        public int GenSameStatPercent { get; set; }
        public int GenSameStatBonusPct { get; set; }
        public int GenGlobalScalingPercent { get; set; }
        public override string Id { get; set; }

        List<ItemType> _primaryReagents = null;
        public List<ItemType> GetPrimaryReagents()
        {
            if (_primaryReagents == null)
            {
                _primaryReagents = _data.Where(x => x.IsReagent() && x.HasFlag(ItemFlags.PrimaryReagent)).ToList();
            }
            return _primaryReagents;
        }

        List<ItemType> _secondaryReagents = null;
        public List<ItemType> GetSecondaryReagents()
        {
            if (_secondaryReagents == null)
            {
                _secondaryReagents = _data.Where(x => x.IsReagent() && !x.HasFlag(ItemFlags.PrimaryReagent)).ToList();
            }
            return _secondaryReagents;
        }
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


