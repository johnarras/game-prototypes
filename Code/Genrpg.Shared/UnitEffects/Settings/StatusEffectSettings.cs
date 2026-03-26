using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UnitEffects.Constants;
using System.Collections.Generic;

namespace Genrpg.Shared.UnitEffects.Settings
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


