using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.TimedEvents.Entities;
using Genrpg.Shared.TimedEvents.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.TimedEvents.Seasons.Settings
{
    public class SeasonTierListSettings : ParentSettings<SeasonTierList>, ITimedEventTierSettings
    {
        public override string Id { get; set; }

        public ITimedEventTierList GetTierList(long id) { return Get(id); }
    }

    public class SeasonTierList : ChildSettings, IIdName, ITimedEventTierList
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public List<TimedEventTier> Tiers { get; set; } = new List<TimedEventTier>();
        public int StartBonusPoints { get; set; }
        public int BonusPointsPerTier { get; set; }
        public long BonusEntityTypeId { get; set; }
        public long BonusEntityId { get; set; }
        public long BonusQuantity { get; set; }
    }

    public class SeasonTierListSettingsDto : ParentSettingsDto<SeasonTierListSettings, SeasonTierList>
    {
        public override List<SeasonTierList> Children { get; set; }
        public override SeasonTierListSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class SeasonTierListSettingsLoader : ParentSettingsLoader<SeasonTierListSettings, SeasonTierList> { }

    public class SeasonTierListSettingsMapper : ParentSettingsMapper<SeasonTierListSettings, SeasonTierList, SeasonTierListSettingsDto> { }


    public class SeasonTierListHelper : BaseEntityHelper<SeasonTierListSettings, SeasonTierList>
    {
        public override long HelperKey => EntityTypes.SeasonTierList;
    }

}


