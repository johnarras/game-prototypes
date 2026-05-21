using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.CaravanMembers.Settings
{
    public class SkinTypeSettings : ParentSettings<SkinType>
    {
        public override string Id { get; set; }
    }

    public class SkinType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public long DefaultSkinForCaravanMemberId { get; set; }
    }

    public class SkinTypeSettingsDto : ParentSettingsDto<SkinTypeSettings, SkinType>
    {
        public override List<SkinType> Children { get; set; }
        public override SkinTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class SkinTypeSettingsLoader : ParentSettingsLoader<SkinTypeSettings, SkinType> { }

    public class SkinTypeSettingsMapper : ParentSettingsMapper<SkinTypeSettings, SkinType, SkinTypeSettingsDto> { }

    public class SkinTypeEntityHelper : BaseEntityHelper<SkinTypeSettings, SkinType>
    {
        public override long HelperKey => EntityTypes.SkinType;
    }
}


