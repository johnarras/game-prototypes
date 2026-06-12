using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Inventory.Constants;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Inventory.Settings.Slots
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

        public bool IsWeaponSlot { get; set; }

    }
    public class EquipSlotSettings : ParentConstantListSettings<EquipSlot, EquipSlots>
    {
        public override string Id { get; set; }


        private List<long> _weaponSlots = null;



        public override void SetData(List<EquipSlot> data)
        {
            base.SetData(data);
            CalcWeaponSlots();
        }

        private void CalcWeaponSlots()
        {
            _weaponSlots = GetData().Where(x => x.IsWeaponSlot).Select(x => x.IdKey).ToList();
        }
        public List<long> GetWeaponSlots()
        {
            if (_weaponSlots == null || _weaponSlots.Count == 0)
            {
                CalcWeaponSlots();
            }
            return _weaponSlots;
        }

        public bool IsWeaponSlot(long equipSlotId)
        {
            return Get(equipSlotId)?.IsWeaponSlot ?? false;
        }

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


