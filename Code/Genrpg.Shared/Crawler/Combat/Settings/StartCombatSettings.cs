using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Crawler.Combat.Settings
{
    public class StartCombatSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }

        public long GroupSizeIncrement { get; set; }

        // Used for the size of each group
        public long StartMaxGroupSize { get; set; }
        public double GroupSizeIncreasePerLevel { get; set; }
        public long MaxGroupSize { get; set; }


        // Used for how many groups there are
        public double BaseGroupCountIncreaseChance { get; set; }
        public double GroupCountIncreaseChancePerLevel { get; set; }
        public double MaxGroupCountIncreaseChance { get; set; }
        public double GroupCountIncreaseMultPerGroupAdded { get; set; }
        public double MaxGroupCountPerLevel { get; set; }
        public long MaxGroupCount { get; set; }

        public double SelectRandomUnitForCombatGroupChance { get; set; }

        public double RangeIncreaseChancePerGroup { get; set; }

    }


    public class StartCombatSettingsLoader : NoChildSettingsLoader<StartCombatSettings> { }

    public class StartCombatSettingsDto : NoChildSettingsDto<StartCombatSettings>
    {
        public override StartCombatSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class StartCombatSettingsMapper : NoChildSettingsMapper<StartCombatSettings, StartCombatSettingsDto> { }
}


