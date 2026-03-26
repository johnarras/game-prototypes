using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.CaravanMembers.Settings
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


