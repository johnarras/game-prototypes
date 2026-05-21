using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.Vendors.Settings
{
    public class VendorSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public float SellToVendorPriceMult { get; set; }
        public float VendorRefreshMinutes { get; set; }
    }

    public class VendorSettingsLoader : NoChildSettingsLoader<VendorSettings> { }


    public class VendorSettingsDto : NoChildSettingsDto<VendorSettings>
    {
        public override string Id { get; set; }
        public override VendorSettings Parent { get; set; }
    }

    public class VendorSettingsMapper : NoChildSettingsMapper<VendorSettings, VendorSettingsDto> { }
}


