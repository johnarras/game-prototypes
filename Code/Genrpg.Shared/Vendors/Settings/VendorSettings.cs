using MessagePack;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.BoardGame.Settings;

namespace Genrpg.Shared.Vendors.Settings
{
    [MessagePackObject]
    public class VendorSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public float SellToVendorPriceMult { get; set; }
        [Key(2)] public float VendorRefreshMinutes { get; set; }
    }

    public class VendorSettingsLoader : NoChildSettingsLoader<VendorSettings> { }


    public class VendorSettingsDto : NoChildSettingsDto<VendorSettings> { }

    public class VendorSettingsMapper : NoChildSettingsMapper<VendorSettings, VendorSettingsDto> { }
}
