using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Names.Settings;
using Genrpg.Shared.ProcGen.Settings.Monsters;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Units.Interfaces;

namespace Genrpg.Shared.Units.Settings
{
    [MessagePackObject]
    public class UnitTypeSettings : ParentSettings<UnitType>
    {
        [Key(0)] public override string Id { get; set; }

    }

    public interface IKeywordList
    {
        List<CurrentUnitKeyword> Keywords { get; set; }
    }

    [MessagePackObject]
    public class UnitType : ChildSettings, IUnitRole, IKeywordList
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

        [Key(9)] public float Height { get; set; }

        [Key(10)] public long TribeTypeId { get; set; }

        [Key(11)] public long MinLevel { get; set; }

        [Key(12)] public int MinRange { get; set; }

        [Key(13)] public double SpawnQuantityScale { get; set; }

        [Key(14)] public List<UnitEffect> Effects { get; set; } = new List<UnitEffect>();

        [Key(15)] public List<WeightedName> PrefixNames { get; set; } = new List<WeightedName>();

        [Key(16)] public List<WeightedName> DoubleNameSuffixes { get; set; } = new List<WeightedName>();

        [Key(17)] public List<WeightedName> SuffixNames { get; set; } = new List<WeightedName>();

        [Key(18)] public List<WeightedName> AlternateNames { get; set; } = new List<WeightedName>();

        [Key(19)] public List<MonsterFood> FoodSources { get; set; } = new List<MonsterFood>();

        [Key(20)] public List<SpawnItem> LootItems { get; set; } = new List<SpawnItem>();
        [Key(21)] public List<SpawnItem> InteractLootItems { get; set; } = new List<SpawnItem>();

        [Key(22)] public List<CurrentUnitKeyword> Keywords { get; set; } = new List<CurrentUnitKeyword>();
    }

        public class UnitTypeSettingsDto : ParentSettingsDto<UnitTypeSettings, UnitType> { }

        public class UnitTypeSettingsLoader : ParentSettingsLoader<UnitTypeSettings, UnitType> { }

        public class UnitTypeSettingsMapper : ParentSettingsMapper<UnitTypeSettings, UnitType, UnitTypeSettingsDto> { }    

    public class UnitTypeHelper : BaseEntityHelper<UnitTypeSettings, UnitType>
    {
        public override long Key => EntityTypes.Unit;
    }
}
