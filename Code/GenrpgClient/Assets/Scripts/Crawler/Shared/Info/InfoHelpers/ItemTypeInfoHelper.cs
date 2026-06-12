using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Inventory.Services;
using OxDb.SharedGame.Inventory.Settings.ItemTypes;
using OxDb.SharedGame.Inventory.Settings.Slots;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Info.InfoHelpers
{
    public class ItemTypeInfoHelper : BaseInfoHelper<ItemTypeSettings, ItemType>
    {

        private ISharedItemService _sharedItemService = null;

        public override long HelperKey => EntityTypes.Item;

        protected override bool MakeEntityNamePlural() { return false; }

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> lines = base.GetInfoLines(entityId);

            ItemType itype = _gameData.Get<ItemTypeSettings>(_gs.ch).Get(entityId);

            if (itype == null)
            {
                return lines;
            }

            EquipSlot slot = _gameData.Get<EquipSlotSettings>(_gs.ch).Get(itype.EquipSlotId);

            if (slot != null)
            {
                lines.Add("Equips in the " + _infoService.CreateInfoLink(slot) + " slot ");
            }



            List<WeaponRoleDamage> roleDams = new List<WeaponRoleDamage>();
            foreach (Effect eff in itype.Effects)
            {
                if (eff.EntityTypeId != EntityTypes.RoleScaling)
                {
                    continue;
                }

                roleDams.Add(_sharedItemService.GetRoleDamage(_gs.ch, itype.IdKey, eff.EntityId));
            }

            foreach (WeaponRoleDamage roleDam in roleDams)
            {
                if (roleDam.MaxDam > 0)
                {
                    lines.Add(roleDam.MinDam + "-" + roleDam.MaxDam + " " + roleDam.DamageName + " Dam");
                }
            }

            return lines;
        }
    }
}


