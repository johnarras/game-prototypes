using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.PlayerData;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.Settings
{
    public class DefaultStoreOfferSettings : NoChildSettings
    {
        public override string Id { get; set; }

        public List<PlayerStoreOffer> Offers { get; set; } = new List<PlayerStoreOffer>();
    }

    public class DefaultStoreOfferSettingsLoader : NoChildSettingsLoader<DefaultStoreOfferSettings> { }

    public class DefaultStoreOfferSettingsDto : NoChildSettingsDto<DefaultStoreOfferSettings>
    {
        public override DefaultStoreOfferSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class DefaultStoreOfferSettingsMapper : NoChildSettingsMapper<DefaultStoreOfferSettings, DefaultStoreOfferSettingsDto> { }
}


