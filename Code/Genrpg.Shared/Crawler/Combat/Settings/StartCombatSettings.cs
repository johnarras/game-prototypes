using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Crawler.Combat.Settings
{
    [MessagePackObject]
    public class StartCombatSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public long GroupSizeIncrement { get; set; }

        // Used for the size of each group
        [Key(2)] public long StartMaxGroupSize { get; set; }
        [Key(3)] public double GroupSizeIncreasePerLevel { get; set; }
        [Key(4)] public long MaxGroupSize { get; set; }


        // Used for how many groups there are
        [Key(5)] public double BaseGroupCountIncreaseChance { get; set; }
        [Key(6)] public double GroupCountIncreaseChancePerLevel { get; set; }
        [Key(7)] public double MaxGroupCountIncreaseChance { get; set; }
        [Key(8)] public double GroupCountIncreaseMultPerGroupAdded { get; set; }
        [Key(9)] public double MaxGroupCountPerLevel { get; set; }
        [Key(10)] public long MaxGroupCount { get; set; }

        [Key(11)] public double SelectRandomUnitForCombatGroupChance { get; set; }

        [Key(12)] public double RangeIncreaseChancePerGroup { get; set; }

    }


    public class StartCombatSettingsLoader : NoChildSettingsLoader<StartCombatSettings> { }

    public class StartCombatSettingsDto : NoChildSettingsDto<StartCombatSettings> { }

    public class StartCombatSettingsMapper : NoChildSettingsMapper<StartCombatSettings, StartCombatSettingsDto> { }
}
