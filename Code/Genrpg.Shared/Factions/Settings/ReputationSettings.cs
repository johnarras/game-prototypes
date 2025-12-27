using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Factions.Settings
{
    public class ReputationSettings : ParentSettings<RepLevel>
    {
        public override string Id { get; set; }
    }

    public class RepLevel : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }

        public string Art { get; set; }


        public int PointsNeeded { get; set; }

    }

    public class ReputationSettingsDto : ParentSettingsDto<ReputationSettings, RepLevel>
    {
        public override List<RepLevel> Children { get; set; }
        public override ReputationSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class ReputationSettingsLoader : ParentSettingsLoader<ReputationSettings, RepLevel> { }

    public class ReputationSettingsMapper : ParentSettingsMapper<ReputationSettings, RepLevel, ReputationSettingsDto> { }

}


