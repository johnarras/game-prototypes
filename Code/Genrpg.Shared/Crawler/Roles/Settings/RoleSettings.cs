using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Roles.Settings
{
    public class AllowedWeapon
    {
        public long ItemTypeId { get; set; }
    }


    public class AllowedEquipSlot
    {
        public long EquipSlotId { get; set; }
    }

    public class RoleBonusBinary
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
    }

    public class RoleBonusAmount
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public double Amount { get; set; }
    }

    public class Role : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public string Abbrev { get; set; }

        public long RoleCategoryId { get; set; }
        public int HealthPerLevel { get; set; }
        public int ManaPerLevel { get; set; }
        public long MaxArmorScalingTypeId { get; set; }
        public long CritPercent { get; set; } = 0;
        public bool Guardian { get; set; } = false;

        public double TrainingXpScale { get; set; }
        public double TrainingGoldScale { get; set; }

        public long StartSkillPoints { get; set; }
        public long StartUpgradePoints { get; set; }

        public List<RoleBonusBinary> BinaryBonuses { get; set; } = new List<RoleBonusBinary>();

        public List<RoleBonusAmount> AmountBonuses { get; set; } = new List<RoleBonusAmount>();
    }


    public class RoleSettings : ParentSettings<Role>
    {
        public override string Id { get; set; }

        public List<Role> GetRoles(List<UnitRole> unitRoles)
        {
            List<Role> roles = new List<Role>();

            foreach (UnitRole uc in unitRoles)
            {
                Role cl = Get(uc.RoleId);
                if (cl != null && !roles.Contains(cl))
                {
                    roles.Add(cl);
                }
            }
            return roles;
        }

        public bool HasBonus(List<UnitRole> roles, long entityTypeId, long entityId)
        {
            foreach (UnitRole uc in roles)
            {
                Role cl = Get(uc.RoleId);
                if (cl != null && cl.BinaryBonuses.Any(x => x.EntityTypeId == entityTypeId && x.EntityId == entityId))
                {
                    return true;
                }
            }
            return false;
        }
    }


    public class RoleSettingsDto : ParentSettingsDto<RoleSettings, Role>
    {
        public override List<Role> Children { get; set; }
        public override RoleSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class RoleSettingsLoader : ParentSettingsLoader<RoleSettings, Role> { }

    public class RoleSettingsMapper : ParentSettingsMapper<RoleSettings, Role, RoleSettingsDto> { }

    public class RoleHelper : BaseEntityHelper<RoleSettings, Role>
    {
        public override long HelperKey => EntityTypes.Role;
    }

}


