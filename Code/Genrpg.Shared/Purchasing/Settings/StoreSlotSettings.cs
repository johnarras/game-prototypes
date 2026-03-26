using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.Settings
{
    public class StoreSlotSettings : ParentSettings<StoreSlot>
    {
        public override string Id { get; set; }
    }

    public class StoreSlotSettingsDto : ParentSettingsDto<StoreSlotSettings, StoreSlot>
    {
        public override List<StoreSlot> Children { get; set; }
        public override StoreSlotSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class StoreSlotSettingsLoader : ParentSettingsLoader<StoreSlotSettings, StoreSlot> { }

    public class StoreSlotSettingsMapper : ParentSettingsMapper<StoreSlotSettings, StoreSlot, StoreSlotSettingsDto> { }


    public class StoreSlot : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string Art { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public bool IsDefaultStore { get; set; }
    }
}


