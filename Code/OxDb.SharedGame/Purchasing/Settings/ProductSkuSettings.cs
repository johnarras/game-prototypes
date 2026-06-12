using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Purchasing.Settings
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
        public string GoogleProductId { get; set; }
        public string AppleProductId { get; set; }
    }
}


