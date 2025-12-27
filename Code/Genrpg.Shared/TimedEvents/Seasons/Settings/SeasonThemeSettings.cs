using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.TimedEvents.Entities;
using Genrpg.Shared.TimedEvents.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.TimedEvents.Seasons.Settings
{
    public class SeasonThemeSettings : ParentSettings<SeasonTheme>, ITimedEventThemeSettings
    {
        public override string Id { get; set; }

        public ITimedEventTheme GetTheme(long themeId) { return Get(themeId); }
    }

    public class SeasonTheme : ChildSettings, ITimedEventTheme
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public List<TimedEventCustomReward> CustomRewards { get; set; } = new List<TimedEventCustomReward>();
    }

    public class SeasonThemeSettingsDto : ParentSettingsDto<SeasonThemeSettings, SeasonTheme>
    {
        public override List<SeasonTheme> Children { get; set; }
        public override SeasonThemeSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class SeasonThemeSettingsLoader : ParentSettingsLoader<SeasonThemeSettings, SeasonTheme> { }

    public class SeasonThemeSettingsMapper : ParentSettingsMapper<SeasonThemeSettings, SeasonTheme, SeasonThemeSettingsDto> { }


    public class SeasonThemeHelper : BaseEntityHelper<SeasonThemeSettings, SeasonTheme>
    {
        public override long HelperKey => EntityTypes.SeasonTierList;
    }

}


