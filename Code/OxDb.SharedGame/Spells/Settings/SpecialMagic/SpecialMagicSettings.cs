using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Spells.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Spells.Settings.SpecialMagic
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


