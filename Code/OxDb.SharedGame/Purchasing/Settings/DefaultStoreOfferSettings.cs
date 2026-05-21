using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedGame.Purchasing.PlayerData;
using System.Collections.Generic;

namespace OxDb.SharedGame.Purchasing.Settings
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


