using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Cultures.Settings
{
    public class CultureTypeSettings : ParentSettings<CultureType>
    {
        public override string Id { get; set; }
        public double CultureTypeChance { get; set; }
    }

    public class CultureType : ChildSettings, IIndexedGameItem
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

    public class CultureTypeSettingsDto : ParentSettingsDto<CultureTypeSettings, CultureType>
    {
        public override List<CultureType> Children { get; set; }
        public override CultureTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CultureTypeSettingsLoader : ParentSettingsLoader<CultureTypeSettings, CultureType> { }

    public class CultureTypeSettingsMapper : ParentSettingsMapper<CultureTypeSettings, CultureType, CultureTypeSettingsDto> { }

}


