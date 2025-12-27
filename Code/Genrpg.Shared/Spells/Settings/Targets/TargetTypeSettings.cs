using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Constants;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Spells.Settings.Targets
{
    /// <summary>
    /// What kind of target a spell has.
    /// 
    /// When crafting spells, Buffs can only be added to other buffs.
    /// But spells with Ally+Enemy parts can both be combined. (like damage+heal)
    /// 
    /// 
    /// </summary>
    public class TargetType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }

        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public double StatBonusScale { get; set; }
    }
    public class TargetTypeSettings : ParentConstantListSettings<TargetType, TargetTypes>
    {
        public override string Id { get; set; }
    }

    public class TargetTypeSettingsDto : ParentSettingsDto<TargetTypeSettings, TargetType>
    {
        public override List<TargetType> Children { get; set; }
        public override TargetTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class TargetTypeSettingsLoader : ParentSettingsLoader<TargetTypeSettings, TargetType> { }

    public class TargetTypeSettingsMapper : ParentSettingsMapper<TargetTypeSettings, TargetType, TargetTypeSettingsDto> { }
}


