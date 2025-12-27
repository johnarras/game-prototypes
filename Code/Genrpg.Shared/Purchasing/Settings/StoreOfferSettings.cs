using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.GameSettings.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.PlayerFiltering.Settings;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.Settings
{
    public class StoreOfferSettings : BaseDataOverrideSettings<StoreOffer>
    {
        public override string Id { get; set; }
    }

    public class StoreOfferSettingsDto : ParentSettingsDto<StoreOfferSettings, StoreOffer>
    {
        public override List<StoreOffer> Children { get; set; }
        public override StoreOfferSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class StoreOfferSettingsLoader : ParentSettingsLoader<StoreOfferSettings, StoreOffer>
    {
    }


    public class StoreOfferSettingsMapper : ParentSettingsMapper<StoreOfferSettings, StoreOffer, StoreOfferSettingsDto>
    {
        public override bool SendToClient() { return false; }
    }

    public class StoreOffer : ChildSettings, IPlayerFilter
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }

        public bool Enabled { get; set; } = true;
        public string Desc { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public string OfferId { get; set; } = HashUtils.NewUUId();

        public long StoreSlotId { get; set; }
        public long StoreFeatureId { get; set; }
        public long StoreThemeId { get; set; }
        public long StoreBundleSetId { get; set; }

        public long TotalModSize { get; set; }
        public long MaxModValue { get; set; }
        public long Priority { get; set; }
        public long MinLevel { get; set; }
        public long MaxLevel { get; set; }
        public long MinPurchaseCount { get; set; }
        public long MaxPurchaseCount { get; set; }
        public double MinPurchaseTotal { get; set; }
        public double MaxPurchaseTotal { get; set; }
        public double MinInstallDays { get; set; }
        public double MaxInstallDays { get; set; }

        public string MinClientVersion { get; set; } = VersionConstants.MinVersion.ToString();
        public string MaxClientVersion { get; set; } = VersionConstants.MaxVersion.ToString();

        public DateTime StartDate { get; set; } = DateTime.MinValue;
        public DateTime EndDate { get; set; } = DateTime.MaxValue;
        public int RepeatHours { get; set; }
        public bool RepeatMonthly { get; set; }

        public List<AllowedPlayer> AllowedPlayers { get; set; } = new List<AllowedPlayer>();
        public bool IsDefaultOffer { get; set; }
        public void DeepCopyFrom(IComplexCopy from, ISerializer serializer)
        {
            OfferId = HashUtils.NewUUId();
        }

        public void OrderSelf()
        {

        }
    }
}


