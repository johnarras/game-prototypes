using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;
using System;
using System.Security;

namespace Genrpg.Shared.Crawler.Settings
{
    [MessagePackObject]
    public class CrawlerSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long MaxPartySize { get; set; }
    }


    public class CrawlerSettingsLoader : NoChildSettingsLoader<CrawlerSettings> { }


    public class CrawlerSettingsDto : NoChildSettingsDto<CrawlerSettings> { }

    public class CrawlerSettingsMapper : NoChildSettingsMapper<CrawlerSettings, CrawlerSettingsDto> { }
}
