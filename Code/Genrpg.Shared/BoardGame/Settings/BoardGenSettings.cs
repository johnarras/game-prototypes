using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.BoardGame.Settings
{
    [MessagePackObject]
    public class BoardGenSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double DistanceBetweenUniqueTiles { get; set; }
        [Key(2)] public double RadDelta { get; set; }

        [Key(3)] public float SlopeMax { get; set; }
        [Key(4)] public int MinPointCount { get; set; }
        [Key(5)] public int MaxPointCount { get; set; }
    }
    

    public class BoardGenSettingsLoader : NoChildSettingsLoader<BoardGenSettings> { }


    public class BoardGenSettingsDto : NoChildSettingsDto<BoardGenSettings> { }

    public class BoardGenSettingsMapper : NoChildSettingsMapper<BoardGenSettings,BoardGenSettingsDto> { }
}
