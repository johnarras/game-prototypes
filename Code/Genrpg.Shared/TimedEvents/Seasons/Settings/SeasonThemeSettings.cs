using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using System.Collections.Generic;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.TimedEvents.Interfaces;
using Genrpg.Shared.TimedEvents.Entities;

namespace Genrpg.Shared.TimedEvents.Seasons.Settings
{
    [MessagePackObject]
    public class SeasonThemeSettings : ParentSettings<SeasonTheme>, ITimedEventThemeSettings
    {
        [Key(0)] public override string Id { get; set; }

        public ITimedEventTheme GetTheme(long themeId) {  return Get(themeId); }
    }

    [MessagePackObject]
    public class SeasonTheme : ChildSettings, ITimedEventTheme
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public List<TimedEventCustomReward> CustomRewards { get; set; } = new List<TimedEventCustomReward>();
    }

    public class SeasonThemeSettingsDto : ParentSettingsDto<SeasonThemeSettings, SeasonTheme> { }

    public class SeasonThemeSettingsLoader : ParentSettingsLoader<SeasonThemeSettings, SeasonTheme> { }

    public class SeasonThemeSettingsMapper : ParentSettingsMapper<SeasonThemeSettings, SeasonTheme, SeasonThemeSettingsDto> { }


    public class SeasonThemeHelper : BaseEntityHelper<SeasonThemeSettings, SeasonTheme>
    {
        public override long Key => EntityTypes.SeasonTierList;
    }

}
