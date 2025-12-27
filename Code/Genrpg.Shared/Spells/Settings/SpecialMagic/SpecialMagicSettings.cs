using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Constants;
using System.Collections.Generic;

namespace Genrpg.Shared.Spells.Settings.SpecialMagic
{
    /// <summary>
    /// What kind of target a spell has.
    /// 
    /// When crafting spells, Buffs can only be added to other buffs.
    /// But spells with Ally+Enemy parts can both be combined. (like damage+heal)
    /// 
    /// 
    /// </summary>
    public class SpecialMagic : ChildSettings, IIndexedGameItem
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
    public class SpecialMagicSettings : ParentConstantListSettings<SpecialMagic, SpecialMagics>
    {
        public override string Id { get; set; }
    }

    public class SpecialMagicSettingsDto : ParentSettingsDto<SpecialMagicSettings, SpecialMagic>
    {
        public override List<SpecialMagic> Children { get; set; }
        public override SpecialMagicSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class SpecialMagicSettingsLoader : ParentSettingsLoader<SpecialMagicSettings, SpecialMagic> { }

    public class SpecialMagicSettingsMapper : ParentSettingsMapper<SpecialMagicSettings, SpecialMagic, SpecialMagicSettingsDto> { }


    public class SpecialMagicEntityHelper : BaseEntityHelper<SpecialMagicSettings, SpecialMagic>
    {
        public override long HelperKey => EntityTypes.SpecialMagic;
    }
}


