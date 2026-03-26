using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Sexes.Constants;
using System.Collections.Generic;

namespace Genrpg.Shared.Sexes.Settings
{
    /// <summary>
    /// List of equipment slots for characters
    /// </summary>
    public class SexType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public long Armor { get; set; }
        public long Damage { get; set; }


        public long CostPercent { get; set; } = 100;

    }

    public class SexTypeSettings : ParentConstantListSettings<SexType, SexTypes>
    {
        public override string Id { get; set; }

        public double LevelsPerQuality { get; set; } = 5.0f;

        public double ExtraQualityChance { get; set; } = 0.25f;
    }

    public class SexTypeSettingsDto : ParentSettingsDto<SexTypeSettings, SexType>
    {
        public override List<SexType> Children { get; set; }
        public override SexTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class SexTypeSettingsLoader : ParentSettingsLoader<SexTypeSettings, SexType> { }

    public class SexTypeSettingsMapper : ParentSettingsMapper<SexTypeSettings, SexType, SexTypeSettingsDto> { }

}


