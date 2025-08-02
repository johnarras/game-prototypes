using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Interfaces;
using Genrpg.Shared.Utils;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Units.Settings
{
    [MessagePackObject]
    public class UnitKeywordSettings : ParentSettings<UnitKeyword>
    {
        [Key(0)] public override string Id { get; set; }

    }
    [MessagePackObject]
    public class UnitKeyword : ChildSettings, IUnitRole, IWeightedItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string PluralName { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string AtlasPrefix { get; set; }
        [Key(7)] public string Icon { get; set; }
        [Key(8)] public string Art { get; set; }

        [Key(9)] public int MinRange { get; set; }
      
        [Key(10)] public List<UnitEffect> Effects { get; set; } = new List<UnitEffect>();

        [Key(11)] public long MinLevel { get; set; }

        [Key(12)] public double Weight { get; set; }

        [Key(13)] public int Tier { get; set; }

        public class UnitKeywordSettingsDto : ParentSettingsDto<UnitKeywordSettings, UnitKeyword> { }

        public class UnitKeywordSettingsLoasder : ParentSettingsLoader<UnitKeywordSettings, UnitKeyword> { }

        public class UnitKeywordTypeSettingsMapper : ParentSettingsMapper<UnitKeywordSettings, UnitKeyword, UnitKeywordSettingsDto> { }
    }
}
