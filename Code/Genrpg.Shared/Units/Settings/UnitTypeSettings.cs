using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Names.Settings;
using Genrpg.Shared.ProcGen.Settings.Monsters;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Units.Settings
{
    public class UnitTypeSettings : ParentSettings<UnitType>
    {
        public override string Id { get; set; }

    }

    public interface IKeywordList
    {
        List<CurrentUnitKeyword> Keywords { get; set; }
    }

    public class UnitType : ChildSettings, IUnitRole, IKeywordList
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string PluralName { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public float Height { get; set; }

        public long TribeTypeId { get; set; }

        public long MinLevel { get; set; }

        public int MinRange { get; set; }

        public double SpawnQuantityScale { get; set; }

        public List<Effect> Effects { get; set; } = new List<Effect>();

        public List<WeightedName> PrefixNames { get; set; } = new List<WeightedName>();

        public List<WeightedName> DoubleNameSuffixes { get; set; } = new List<WeightedName>();

        public List<WeightedName> SuffixNames { get; set; } = new List<WeightedName>();

        public List<WeightedName> AlternateNames { get; set; } = new List<WeightedName>();

        public List<MonsterFood> FoodSources { get; set; } = new List<MonsterFood>();

        public List<SpawnItem> LootItems { get; set; } = new List<SpawnItem>();
        public List<SpawnItem> InteractLootItems { get; set; } = new List<SpawnItem>();

        public List<CurrentUnitKeyword> Keywords { get; set; } = new List<CurrentUnitKeyword>();
    }

    public class UnitTypeSettingsDto : ParentSettingsDto<UnitTypeSettings, UnitType>
    {
        public override List<UnitType> Children { get; set; }
        public override UnitTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class UnitTypeSettingsLoader : ParentSettingsLoader<UnitTypeSettings, UnitType> { }

    public class UnitTypeSettingsMapper : ParentSettingsMapper<UnitTypeSettings, UnitType, UnitTypeSettingsDto> { }

    public class UnitTypeHelper : BaseEntityHelper<UnitTypeSettings, UnitType>
    {
        public override long HelperKey => EntityTypes.Unit;
    }

    public class PolymorphHelper : BaseEntityHelper<UnitTypeSettings, UnitType>
    {
        public override long HelperKey => EntityTypes.Polymorph;
    }
}


