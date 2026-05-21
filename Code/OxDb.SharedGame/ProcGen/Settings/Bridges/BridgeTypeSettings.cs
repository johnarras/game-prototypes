using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.ProcGen.Settings.Bridges
{
    public class BridgeType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public int Length { get; set; }

        public BridgeType()
        {
            Art = "Bridge";
            Length = 6;
        }

    }
    public class BridgeTypeSettings : ParentSettings<BridgeType>
    {
        public override string Id { get; set; }
    }

    public class BridgeTypeSettingsDto : ParentSettingsDto<BridgeTypeSettings, BridgeType>
    {
        public override List<BridgeType> Children { get; set; }
        public override BridgeTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class BridgeTypeSettingsLoader : ParentSettingsLoader<BridgeTypeSettings, BridgeType> { }

    public class BridgeSettingsMapper : ParentSettingsMapper<BridgeTypeSettings, BridgeType, BridgeTypeSettingsDto> { }


}


