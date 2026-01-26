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

namespace Genrpg.Shared.TimedEvents.Collections.Settings
{
    public class CollectionTierListSettings : ParentSettings<CollectionTierList>, ITimedEventTierSettings
    {
        public override string Id { get; set; }

        public ITimedEventTierList GetTierList(long id) { return Get(id); }
    }

    public class CollectionTierList : ChildSettings, IIdName, ITimedEventTierList
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

    public class CollectionTierListSettingsDto : ParentSettingsDto<CollectionTierListSettings, CollectionTierList>
    {
        public override List<CollectionTierList> Children { get; set; }
        public override CollectionTierListSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CollectionTierListSettingsLoader : ParentSettingsLoader<CollectionTierListSettings, CollectionTierList> { }

    public class CollectionTierListSettingsMapper : ParentSettingsMapper<CollectionTierListSettings, CollectionTierList, CollectionTierListSettingsDto> { }


    public class CollectionTierListHelper : BaseEntityHelper<CollectionTierListSettings, CollectionTierList>
    {
        public override long HelperKey => EntityTypes.CollectionTierList;
    }

}


