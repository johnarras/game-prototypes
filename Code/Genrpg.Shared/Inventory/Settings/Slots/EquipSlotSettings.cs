using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Settings.Slots
{
    /// <summary>
    /// List of equipment slots for characters
    /// </summary>
    [MessagePackObject]
    public class EquipSlot : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }


        /// <summary>
        /// Add a second slot to the given item type.
        /// </summary>
        [Key(7)] public long ParentEquipSlotId { get; set; }

        [Key(8)] public string Art { get; set; }

        [Key(9)] public bool Active { get; set; }

        [Key(10)] public bool IsCrawlerSlot { get; set; }

        [Key(11)] public long BaseBonusStatTypeId { get; set; }

        public EquipSlot()
        {
        }


    }
    [MessagePackObject]
    public class EquipSlotSettings : ParentConstantListSettings<EquipSlot,EquipSlots>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class EquipSlotSettingsDto : ParentSettingsDto<EquipSlotSettings, EquipSlot> { }
    public class EquipSlotSettingsLoader : ParentSettingsLoader<EquipSlotSettings, EquipSlot> { }

    public class EquipSlotSettingsMapper : ParentSettingsMapper<EquipSlotSettings, EquipSlot, EquipSlotSettingsDto> { }

}
