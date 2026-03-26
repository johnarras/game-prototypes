using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.PlayerFiltering.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.GameSettings.Settings
{
    public class DataOverrideSettings : BaseDataOverrideSettings<DataOverrideGroup>
    {
        public override string Id { get; set; }
    }

    public class DataOverrideGroup : ChildSettings, IPlayerFilter
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public bool Enabled { get; set; } = true;

        public long TotalModSize { get; set; }
        public long MaxModValue { get; set; }
        public long Priority { get; set; }

        public double MinInstallDays { get; set; }
        public double MaxInstallDays { get; set; }
        public long MinLevel { get; set; }
        public long MaxLevel { get; set; }
        public long MinPurchaseCount { get; set; }
        public long MaxPurchaseCount { get; set; }
        public double MinPurchaseTotal { get; set; }
        public double MaxPurchaseTotal { get; set; }

        public DateTime StartDate { get; set; } = DateTime.MinValue;
        public DateTime EndDate { get; set; } = DateTime.MaxValue;
        public int RepeatHours { get; set; }
        public bool RepeatMonthly { get; set; }

        public string MaxClientVersion { get; set; } = VersionConstants.MaxVersion.ToString();
        public string MinClientVersion { get; set; } = VersionConstants.MinVersion.ToString();

        public List<DataOverrideItem> Items { get; set; } = new List<DataOverrideItem>();
        public List<AllowedPlayer> AllowedPlayers { get; set; } = new List<AllowedPlayer>();

        public void OrderSelf()
        {
            Items = Items.OrderBy(x => x.SettingsNameId).ThenBy(x => x.DocId).ToList();
        }

    }

    public class DataOverrideItem
    {
        public bool Enabled { get; set; } = true;
        public long SettingsNameId { get; set; }
        public string DocId { get; set; }
        public string Name { get; set; }
    }

    public class DataOverrideItemPriority
    {
        public long SettingsNameId { get; set; }
        public string DocId { get; set; }
        public long Priority { get; set; }
        public string Name { get; set; }
    }

    public class DataOverrideSettingsDto : ParentSettingsDto<DataOverrideSettings, DataOverrideGroup>
    {
        public override List<DataOverrideGroup> Children { get; set; }
        public override DataOverrideSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class DataOverrideLoader : ParentSettingsLoader<DataOverrideSettings, DataOverrideGroup> { }

    public class DataOverrideSettingsMapper : ParentSettingsMapper<DataOverrideSettings, DataOverrideGroup, DataOverrideSettingsDto> { }


}


