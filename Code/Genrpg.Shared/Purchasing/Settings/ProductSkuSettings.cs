using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Purchasing.Settings
{
    [MessagePackObject]
    public class ProductSkuSettings : ParentSettings<ProductSku>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class ProductSkuSettingsDto : ParentSettingsDto<ProductSkuSettings, ProductSku> { }
    public class ProductSkuSettingsLoader : ParentSettingsLoader<ProductSkuSettings, ProductSku> { }

    public class BridgeSettingsMapper : ParentSettingsMapper<ProductSkuSettings, ProductSku, ProductSkuSettingsDto> { }


    public class ProductSkuEntityHelper : BaseEntityHelper<ProductSkuSettings, ProductSku>
    {
        public override long Key => EntityTypes.ProductSku;
    }


    [MessagePackObject]
    public class ProductSku : ChildSettings, IIdName
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public double DollarPrice { get; set; }
        [Key(6)] public long GemPrice { get; set; }
        [Key(7)] public string GoogleProductId { get; set; }
        [Key(8)] public string AppleProductId { get; set; }
    }
}
