using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Buildings.Settings
{
    [MessagePackObject]
    public class BuildingArtSettings : ParentSettings<BuildingArt>
    {
        [Key(0)] public override string Id { get; set; }
    }
    [MessagePackObject]
    public class BuildingArt : ChildSettings, IIndexedGameItem, IWeightedItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public double Weight { get; set; }

    }

    public class BuildingArtSettingsDto : ParentSettingsDto<BuildingArtSettings, BuildingArt> { }

    public class BuildingArtSettingsLoader : ParentSettingsLoader<BuildingArtSettings, BuildingArt> { }

    public class BuildingArtSettingsMapper : ParentSettingsMapper<BuildingArtSettings, BuildingArt, BuildingArtSettingsDto> { }


}
