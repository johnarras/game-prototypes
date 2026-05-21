using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Units.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Units.Settings
{
    public class UnitKeywordSettings : ParentSettings<UnitKeyword>
    {
        public override string Id { get; set; }

    }
    public class UnitKeyword : ChildSettings, IUnitRole, IWeightedItem
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

        public int MinRange { get; set; }

        public List<Effect> Effects { get; set; } = new List<Effect>();

        public long MinLevel { get; set; }

        public double Weight { get; set; }

        public int Tier { get; set; }

    }
    public class UnitKeywordSettingsDto : ParentSettingsDto<UnitKeywordSettings, UnitKeyword>
    {
        public override List<UnitKeyword> Children { get; set; }
        public override UnitKeywordSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class UnitKeywordSettingsLoasder : ParentSettingsLoader<UnitKeywordSettings, UnitKeyword> { }

    public class UnitKeywordTypeSettingsMapper : ParentSettingsMapper<UnitKeywordSettings, UnitKeyword, UnitKeywordSettingsDto> { }
}


