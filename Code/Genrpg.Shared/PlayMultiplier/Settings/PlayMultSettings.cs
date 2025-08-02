using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.PlayMultiplier.Settings
{

    [MessagePackObject]
    public class PlayMultSettings : ParentSettings<PlayMult>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double AvgRollChangePercent { get; set; } = 0.05f;
    }
    [MessagePackObject]
    public class PlayMult : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public long Mult { get; set; }
        [Key(9)] public long MinLevel { get; set; }
        [Key(10)] public long MinEnergy { get; set; }
    }

    public class PlayMultSettingsDto : ParentSettingsDto<PlayMultSettings, PlayMult> { }
    public class PlayMultSettingsLoader : ParentSettingsLoader<PlayMultSettings, PlayMult> { }

    public class PlayMultSettingsMapper : ParentSettingsMapper<PlayMultSettings, PlayMult, PlayMultSettingsDto> { }

}
