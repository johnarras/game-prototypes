using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Trader.Holdings.Settings
{
    [MessagePackObject]
    public class HoldingsSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
    }


    public class HoldingsSettingsLoader : NoChildSettingsLoader<HoldingsSettings> { }


    public class HoldingsSettingsDto : NoChildSettingsDto<HoldingsSettings> { }

    public class HoldingsSettingsMapper : NoChildSettingsMapper<HoldingsSettings, HoldingsSettingsDto> { }
}
