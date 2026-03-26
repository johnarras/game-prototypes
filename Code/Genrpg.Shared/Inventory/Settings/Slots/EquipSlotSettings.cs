using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Constants;
using System.Collections.Generic;

namespace Genrpg.Shared.Inventory.Settings.Slots
{
    /// <summary>
    /// List of equipment slots for characters
    /// </summary>
    public class EquipSlot : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }


        /// <summary>
        /// Add a second slot to the given item type.
        /// </summary>
        public long ParentEquipSlotId { get; set; }

        public string Art { get; set; }

        public bool Active { get; set; }

        public bool IsCrawlerSlot { get; set; }

        public long BaseBonusStatTypeId { get; set; }

        public double BonusStatScale { get; set; }

        public EquipSlot()
        {
        }


    }
    public class EquipSlotSettings : ParentConstantListSettings<EquipSlot, EquipSlots>
    {
        public override string Id { get; set; }
    }

    public class EquipSlotSettingsDto : ParentSettingsDto<EquipSlotSettings, EquipSlot>
    {
        public override List<EquipSlot> Children { get; set; }
        public override EquipSlotSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class EquipSlotSettingsLoader : ParentSettingsLoader<EquipSlotSettings, EquipSlot> { }

    public class EquipSlotSettingsMapper : ParentSettingsMapper<EquipSlotSettings, EquipSlot, EquipSlotSettingsDto> { }

}


