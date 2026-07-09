using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.MapGen.Constants;
using OxDb.SharedGame.DataStores.Categories.ContentData;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.MapGen.Settings
{

    public class EdgePattern : IWeightedItem, IId
    {
        public long IdKey { get; set; }
        public string Name { get; set; }
        public double Weight { get; set; }

        public int Quantity { get; set; }
        public float SymmetricChance { get; set; }

        public int MinTypes { get; set; }
        public int MaxTypes { get; set; }
    }


    public class RoomEdgeTypeSettings : ParentConstantListSettings<RoomEdgeType,RoomEdgeTypes>
    {
        public override string Id { get; set; }

        public List<EdgePattern> EdgePatterns { get; set; } = new List<EdgePattern>();

    }

    public class RoomEdgeType : ChildSettings, IIndexedGameItem, IWeightedItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public double Weight { get; set; }
        public float OffsetChance { get; set; }
        public float NarrowChance { get; set; }
        public float MinDepthRatio { get; set; }
        public float MaxDepthRatio { get; set; }
        public float MinEndDoorChance { get; set; }
        public float MaxEndDoorChance { get; set; }
        public float MinMissingChance { get; set; }
        public float MaxMissingChance { get; set; }
    }

    public class RoomEdgeTypeSettingsDto : ParentSettingsDto<RoomEdgeTypeSettings, RoomEdgeType>
    {
        public override List<RoomEdgeType> Children { get; set; }
        public override RoomEdgeTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class RoomEdgeTypeSettingsLoader : ParentSettingsLoader<RoomEdgeTypeSettings, RoomEdgeType> { }

    public class RoomEdgeTypeSettingsMapper : ParentSettingsMapper<RoomEdgeTypeSettings, RoomEdgeType, RoomEdgeTypeSettingsDto> { }

    public class RoomEdgeTypeEntityHelper : BaseEntityHelper<RoomEdgeTypeSettings, RoomEdgeType>
    {
        public override long HelperKey => EntityTypes.RoomEdge;
    }
}


