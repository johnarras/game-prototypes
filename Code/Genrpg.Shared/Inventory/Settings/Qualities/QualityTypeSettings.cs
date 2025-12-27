using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Inventory.Settings.Qualities
{
    public class QualityType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        // Scaling for generating items.
        public int ItemSpawnWeight { get; set; }
        public int ItemMinLevel { get; set; }
        public int ItemStatPct { get; set; }
        public int ItemCostPct { get; set; }

        // Scaling for generating monsters.
        public string UnitName { get; set; }
        public int UnitSpawnWeight { get; set; }
        public int UnitMinLevel { get; set; }
        public int UnitHealthPct { get; set; }
        public int UnitDamPct { get; set; }
    }

    public class QualityName
    {
        public long QualityTypeId { get; set; }
        public string Name { get; set; }
    }

    public class QualityTypeSettings : ParentSettings<QualityType>
    {
        public override string Id { get; set; }
    }

    public class QualityTypeSettingsDto : ParentSettingsDto<QualityTypeSettings, QualityType>
    {
        public override List<QualityType> Children { get; set; }
        public override QualityTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class QualityTypeSettingsLoader : ParentSettingsLoader<QualityTypeSettings, QualityType> { }

    public class QualitySettingsMapper : ParentSettingsMapper<QualityTypeSettings, QualityType, QualityTypeSettingsDto> { }

}


