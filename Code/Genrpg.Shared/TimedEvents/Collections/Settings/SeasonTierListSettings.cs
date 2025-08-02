using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using System.Collections.Generic;
using Genrpg.Shared.TimedEvents.Entities;
using Genrpg.Shared.TimedEvents.Interfaces;

namespace Genrpg.Shared.TimedEvents.Collections.Settings
{
    [MessagePackObject]
    public class CollectionTierListSettings : ParentSettings<CollectionTierList>, ITimedEventTierSettings
    {
        [Key(0)] public override string Id { get; set; }

        public ITimedEventTierList GetTierList(long id) { return Get(id); }
    }

    [MessagePackObject]
    public class CollectionTierList : ChildSettings, IIdName, ITimedEventTierList
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public List<TimedEventTier> Tiers { get; set; } = new List<TimedEventTier>();
        [Key(5)] public int StartBonusPoints { get; set; }
        [Key(6)] public int BonusPointsPerTier { get; set; }
        [Key(7)] public long BonusEntityTypeId { get; set; }
        [Key(8)] public long BonusEntityId { get; set; }
        [Key(9)] public long BonusQuantity { get; set; }
    }

    public class CollectionTierListSettingsDto : ParentSettingsDto<CollectionTierListSettings, CollectionTierList> { }

    public class CollectionTierListSettingsLoader : ParentSettingsLoader<CollectionTierListSettings, CollectionTierList> { }

    public class CollectionTierListSettingsMapper : ParentSettingsMapper<CollectionTierListSettings, CollectionTierList, CollectionTierListSettingsDto> { }


    public class CollectionTierListHelper : BaseEntityHelper<CollectionTierListSettings, CollectionTierList>
    {
        public override long Key => EntityTypes.CollectionTierList;
    }

}
