using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.GameSettings.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.PlayerFiltering.Settings;
using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.Settings
{
    [MessagePackObject]
    public class StoreOfferSettings : BaseDataOverrideSettings<StoreOffer>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class StoreOfferSettingsDto : ParentSettingsDto<StoreOfferSettings, StoreOffer> { }
    public class StoreOfferSettingsLoader : ParentSettingsLoader<StoreOfferSettings, StoreOffer>
    {
    }


    public class StoreOfferSettingsMapper : ParentSettingsMapper<StoreOfferSettings, StoreOffer, StoreOfferSettingsDto>
    {
        public override bool SendToClient() { return false; }
    }

    [MessagePackObject]
    public class StoreOffer : ChildSettings, IPlayerFilter
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }

        [Key(4)] public bool Enabled { get; set; } = true;
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public string OfferId { get; set; } = HashUtils.NewUUId();

        [Key(12)] public long StoreSlotId { get; set; }
        [Key(13)] public long StoreFeatureId { get; set; }
        [Key(14)] public long StoreThemeId { get; set; }
        [Key(15)] public long StoreBundleSetId { get; set; }

        [Key(9)] public long TotalModSize { get; set; }
        [Key(10)] public long MaxModValue { get; set; }
        [Key(11)] public long Priority { get; set; }
        [Key(18)] public long MinLevel { get; set; }
        [Key(19)] public long MaxLevel { get; set; }
        [Key(20)] public long MinPurchaseCount { get; set; }
        [Key(21)] public long MaxPurchaseCount { get; set; }
        [Key(22)] public double MinPurchaseTotal { get; set; }
        [Key(23)] public double MaxPurchaseTotal { get; set; }
        [Key(16)] public double MinInstallDays { get; set; }
        [Key(17)] public double MaxInstallDays { get; set; }

        [Key(24)] public string MinClientVersion { get; set; } = VersionConstants.MinVersion.ToString();
        [Key(25)] public string MaxClientVersion { get; set; } = VersionConstants.MaxVersion.ToString();

        [Key(27)] public DateTime StartDate { get; set; } = DateTime.MinValue;
        [Key(28)] public DateTime EndDate { get; set; } = DateTime.MaxValue;
        [Key(29)] public int RepeatHours { get; set; }
        [Key(30)] public bool RepeatMonthly { get; set; }

        [Key(31)] public List<AllowedPlayer> AllowedPlayers { get; set; } = new List<AllowedPlayer>();
        public void DeepCopyFrom(IComplexCopy from, ISerializer serializer)
        {
            OfferId = HashUtils.NewUUId();
        }

        public void OrderSelf()
        {

        }
    }
}
