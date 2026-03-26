using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using System.Collections.Generic;

namespace Genrpg.Shared.Tiles.Settings
{
    public class TileTypeSettings : ParentSettings<TileType>
    {
        public override string Id { get; set; }

    }


    public class TileType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public List<SpawnItem> Rewards { get; set; } = new List<SpawnItem>();

    }

    public class TileTypeSettingsDto : ParentSettingsDto<TileTypeSettings, TileType>
    {
        public override List<TileType> Children { get; set; }
        public override TileTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TileTypeSettingsLoader : ParentSettingsLoader<TileTypeSettings, TileType> { }

    public class TileTypeSettingsMapper : ParentSettingsMapper<TileTypeSettings, TileType, TileTypeSettingsDto> { }
}


