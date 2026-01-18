using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Biomes.Settings
{
    public class BiomeTypeSettings : ParentSettings<BiomeType>
    {
        public override string Id { get; set; }
        public double BiomeTypeChance { get; set; }
    }

    public class BiomeType : ChildSettings, IIndexedGameItem
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

    public class BiomeTypeSettingsDto : ParentSettingsDto<BiomeTypeSettings, BiomeType>
    {
        public override List<BiomeType> Children { get; set; }
        public override BiomeTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class BiomeTypeSettingsLoader : ParentSettingsLoader<BiomeTypeSettings, BiomeType> { }

    public class BiomeTypeSettingsMapper : ParentSettingsMapper<BiomeTypeSettings, BiomeType, BiomeTypeSettingsDto> { }

}


