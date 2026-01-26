using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.TimedEvents.Entities;
using Genrpg.Shared.TimedEvents.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.TimedEvents.Collections.Settings
{
    public class CollectionThemeSettings : ParentSettings<CollectionTheme>, ITimedEventThemeSettings
    {
        public override string Id { get; set; }

        public ITimedEventTheme GetTheme(long themeId) { return Get(themeId); }
    }

    public class CollectionTheme : ChildSettings, ITimedEventTheme
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

    public class CollectionThemeSettingsDto : ParentSettingsDto<CollectionThemeSettings, CollectionTheme>
    {
        public override List<CollectionTheme> Children { get; set; }
        public override CollectionThemeSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CollectionThemeSettingsLoader : ParentSettingsLoader<CollectionThemeSettings, CollectionTheme> { }

    public class CollectionThemeSettingsMapper : ParentSettingsMapper<CollectionThemeSettings, CollectionTheme, CollectionThemeSettingsDto> { }


    public class CollectionThemeHelper : BaseEntityHelper<CollectionThemeSettings, CollectionTheme>
    {
        public override long HelperKey => EntityTypes.CollectionTierList;
    }

}


