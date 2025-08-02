using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Sexes.Constants;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Sexes.Settings
{
    /// <summary>
    /// List of equipment slots for characters
    /// </summary>
    [MessagePackObject]
    public class SexType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }

        [Key(8)] public long Armor { get; set; }
        [Key(9)] public long Damage { get; set; }


        [Key(10)] public long CostPercent { get; set; } = 100;

    }

    [MessagePackObject]
    public class SexTypeSettings : ParentConstantListSettings<SexType,SexTypes>
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public double LevelsPerQuality { get; set; } = 5.0f;

        [Key(2)] public double ExtraQualityChance { get; set; } = 0.25f;
    }

    public class SexTypeSettingsDto : ParentSettingsDto<SexTypeSettings, SexType> { }
    public class SexTypeSettingsLoader : ParentSettingsLoader<SexTypeSettings, SexType> { }

    public class SexTypeSettingsMapper : ParentSettingsMapper<SexTypeSettings, SexType, SexTypeSettingsDto> { }

}
