using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.UnitEffects.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.UnitEffects.Settings
{

    public class StatusEffect : ChildSettings, IIndexedGameItem
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
        public long ElementTypeId { get; set; }
        public long CombatActionId { get; set; }
        public long RoleScalingTypeId { get; set; }
        public long Amount { get; set; }
        public bool RemoveAtEndOfCombat { get; set; }
    }

    public class StatusEffectSettings : ParentConstantListSettings<StatusEffect, StatusEffects>
    {
        public override string Id { get; set; }
    }

    public class StatusEffectSettingsDto : ParentSettingsDto<StatusEffectSettings, StatusEffect>
    {
        public override List<StatusEffect> Children { get; set; }
        public override StatusEffectSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class StatusEffectSettingsLoader : ParentSettingsLoader<StatusEffectSettings, StatusEffect> { }

    public class StatusEffectSettingsMapper : ParentSettingsMapper<StatusEffectSettings, StatusEffect, StatusEffectSettingsDto> { }

    public class StatusEffectEntityHelper : BaseEntityHelper<StatusEffectSettings, StatusEffect>
    {
        public override long HelperKey => EntityTypes.StatusEffect;
    }
}


