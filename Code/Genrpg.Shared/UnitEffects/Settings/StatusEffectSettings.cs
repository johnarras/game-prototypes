using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.UnitEffects.Constants;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.UnitEffects.Settings
{

    [MessagePackObject]
    public class StatusEffect : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public string Abbrev { get; set; }
        [Key(9)] public long ElementTypeId { get; set; }
        [Key(10)] public long CombatActionId { get; set; }
        [Key(11)] public long Amount { get; set; }
        [Key(12)] public bool RemoveAtEndOfCombat { get; set; }
    }

    [MessagePackObject]
    public class StatusEffectSettings : ParentConstantListSettings<StatusEffect,StatusEffects>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class StatusEffectSettingsDto : ParentSettingsDto<StatusEffectSettings, StatusEffect> { }
    public class StatusEffectSettingsLoader : ParentSettingsLoader<StatusEffectSettings, StatusEffect> { }

    public class StatusEffectSettingsMapper : ParentSettingsMapper<StatusEffectSettings, StatusEffect, StatusEffectSettingsDto> { }

    public class StatusEffectEntityHelper : BaseEntityHelper<StatusEffectSettings, StatusEffect>
    {
        public override long Key => EntityTypes.StatusEffect;
    }
}
