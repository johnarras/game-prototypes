using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.Maps.Settings
{
    public class IndexedColorSettings : ParentSettings<IndexedColor>
    {
        public override string Id { get; set; }
    }

    public class IndexedColor : ChildSettings, IIdName
    {

        public override string Id { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public override string ParentId { get; set; }
        public long TextureTypeId { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }

    public class IndexedColorSettingsDto : ParentSettingsDto<IndexedColorSettings, IndexedColor>
    {
        public override List<IndexedColor> Children { get; set; }
        public override IndexedColorSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class IndexedColorSettingsLoader : ParentSettingsLoader<IndexedColorSettings, IndexedColor> { }

    public class IndexedColorSettingsMapper : ParentSettingsMapper<IndexedColorSettings, IndexedColor, IndexedColorSettingsDto> { }

}


