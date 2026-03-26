using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Vendors.Settings
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


