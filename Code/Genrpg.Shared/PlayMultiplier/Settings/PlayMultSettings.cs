using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.PlayMultiplier.Settings
{

    public class PlayMultSettings : ParentSettings<PlayMult>
    {
        public override string Id { get; set; }
        public double AvgRollChangePercent { get; set; } = 0.05f;
    }
    public class PlayMult : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public long Mult { get; set; }
        public long MinLevel { get; set; }
        public long MinEnergy { get; set; }
        public long BonusDistancePerDie { get; set; }
    }

    public class PlayMultSettingsDto : ParentSettingsDto<PlayMultSettings, PlayMult>
    {
        public override List<PlayMult> Children { get; set; }
        public override PlayMultSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class PlayMultSettingsLoader : ParentSettingsLoader<PlayMultSettings, PlayMult> { }

    public class PlayMultSettingsMapper : ParentSettingsMapper<PlayMultSettings, PlayMult, PlayMultSettingsDto> { }

}


