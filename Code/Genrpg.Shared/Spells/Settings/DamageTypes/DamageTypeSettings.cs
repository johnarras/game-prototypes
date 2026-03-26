using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Spells.Settings.DamageTypes
{
    /// <summary>
    /// What kind of target a spell has.
    /// 
    /// When crafting spells, Buffs can only be added to other buffs.
    /// But spells with Ally+Enemy parts can both be combined. (like damage+heal)
    /// 
    /// 
    /// </summary>
    public class Damage : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }

        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
    }
    public class DamageSettings : ParentSettings<Damage>
    {
        public override string Id { get; set; }
    }

    public class DamageSettingsDto : ParentSettingsDto<DamageSettings, Damage>
    {
        public override List<Damage> Children { get; set; }
        public override DamageSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class DamageSettingsLoader : ParentSettingsLoader<DamageSettings, Damage> { }

    public class DamageSettingsMapper : ParentSettingsMapper<DamageSettings, Damage, DamageSettingsDto> { }
}


