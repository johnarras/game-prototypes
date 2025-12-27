using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.Settings
{
    public class ProductSkuSettings : ParentSettings<ProductSku>
    {
        public override string Id { get; set; }
    }

    public class ProductSkuSettingsDto : ParentSettingsDto<ProductSkuSettings, ProductSku>
    {
        public override List<ProductSku> Children { get; set; }
        public override ProductSkuSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class ProductSkuSettingsLoader : ParentSettingsLoader<ProductSkuSettings, ProductSku> { }

    public class BridgeSettingsMapper : ParentSettingsMapper<ProductSkuSettings, ProductSku, ProductSkuSettingsDto> { }


    public class ProductSkuEntityHelper : BaseEntityHelper<ProductSkuSettings, ProductSku>
    {
        public override long HelperKey => EntityTypes.ProductSku;
    }


    public class ProductSku : ChildSettings, IIdName
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public double DollarPrice { get; set; }
        public long GemPrice { get; set; }
        public string GoogleProductId { get; set; }
        public string AppleProductId { get; set; }
    }
}


