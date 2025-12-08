using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.PlayerFiltering.Settings;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.GameSettings.Settings
{
    [MessagePackObject]
    public class DataOverrideSettings : BaseDataOverrideSettings<DataOverrideGroup>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class DataOverrideGroup : ChildSettings, IPlayerFilter
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public bool Enabled { get; set; } = true;

        [Key(5)] public long TotalModSize { get; set; }
        [Key(6)] public long MaxModValue { get; set; }
        [Key(7)] public long Priority { get; set; }

        [Key(8)] public double MinInstallDays { get; set; }
        [Key(9)] public double MaxInstallDays { get; set; }
        [Key(10)] public long MinLevel { get; set; }
        [Key(11)] public long MaxLevel { get; set; }
        [Key(12)] public long MinPurchaseCount { get; set; }
        [Key(13)] public long MaxPurchaseCount { get; set; }
        [Key(14)] public double MinPurchaseTotal { get; set; }
        [Key(15)] public double MaxPurchaseTotal { get; set; }

        [Key(16)] public DateTime StartDate { get; set; } = DateTime.MinValue;
        [Key(17)] public DateTime EndDate { get; set; } = DateTime.MaxValue;
        [Key(18)] public int RepeatHours { get; set; }
        [Key(19)] public bool RepeatMonthly { get; set; }

        [Key(20)] public string MaxClientVersion { get; set; } = VersionConstants.MaxVersion.ToString();
        [Key(21)] public string MinClientVersion { get; set; } = VersionConstants.MinVersion.ToString();

        [Key(22)] public List<DataOverrideItem> Items { get; set; } = new List<DataOverrideItem>();
        [Key(23)] public List<AllowedPlayer> AllowedPlayers { get; set; } = new List<AllowedPlayer>();

        public void OrderSelf()
        {
            Items = Items.OrderBy(x => x.SettingsNameId).ThenBy(x => x.DocId).ToList();
        }

    }

    [MessagePackObject]
    public class DataOverrideItem
    {
        [Key(0)] public bool Enabled { get; set; } = true;
        [Key(1)] public long SettingsNameId { get; set; }
        [Key(2)] public string DocId { get; set; }
        [Key(3)] public string Name { get; set; }
    }

    [MessagePackObject]
    public class DataOverrideItemPriority
    {
        [Key(0)] public long SettingsNameId { get; set; }
        [Key(1)] public string DocId { get; set; }
        [Key(2)] public long Priority { get; set; }
        [Key(3)] public string Name { get; set; }
    }

    public class DataOverrideSettingsDto : ParentSettingsDto<DataOverrideSettings, DataOverrideGroup> { }
    public class DataOverrideLoader : ParentSettingsLoader<DataOverrideSettings, DataOverrideGroup> { }

    public class DataOverrideSettingsMapper : ParentSettingsMapper<DataOverrideSettings, DataOverrideGroup, DataOverrideSettingsDto> { }


}
