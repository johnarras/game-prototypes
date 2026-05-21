using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using System.Collections.Generic;

namespace OxDb.SharedGame.Stats.Settings.DerivedStats
{
    public class DerivedStat : ChildSettings
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public override string Name { get; set; }
        public long FromStatTypeId { get; set; }
        public long ToStatTypeId { get; set; }
        public int Percent { get; set; }
    }

    public class DerivedStatSettings : ParentSettings<DerivedStat>
    {
        public override string Id { get; set; }
    }

    public class DerivedStatSettingsDto : ParentSettingsDto<DerivedStatSettings, DerivedStat>
    {
        public override List<DerivedStat> Children { get; set; }
        public override DerivedStatSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class DerivedStatSettingLoader : ParentSettingsLoader<DerivedStatSettings, DerivedStat> { }

    public class DerivedStatSettingsMapper : ParentSettingsMapper<DerivedStatSettings, DerivedStat, DerivedStatSettingsDto> { }
}


