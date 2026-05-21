using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Purchasing.Settings
{
    public class StoreFeatureSettings : ParentSettings<StoreFeature>
    {
        public override string Id { get; set; }
    }

    public class StoreFeatureSettingsDto : ParentSettingsDto<StoreFeatureSettings, StoreFeature>
    {
        public override List<StoreFeature> Children { get; set; }
        public override StoreFeatureSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class StoreFeatureSettingsLoader : ParentSettingsLoader<StoreFeatureSettings, StoreFeature> { }

    public class StoreFeatureSettingsMapper : ParentSettingsMapper<StoreFeatureSettings, StoreFeature, StoreFeatureSettingsDto> { }



    public class StoreFeature : ChildSettings, IIdName
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
    }
}


