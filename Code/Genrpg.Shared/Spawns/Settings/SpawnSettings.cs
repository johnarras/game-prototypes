using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Spawns.Settings
{
    public interface ISpawnItem : IWeightedItem
    {
        long EntityTypeId { get; }
        long EntityId { get; }
        long MinQuantity { get; }
        long MaxQuantity { get; }
        int GroupId { get; }
        string Name { get; }
        long MinLevel { get; }
    }


    public class SpawnItem : ISpawnItem
    {
        public string Name { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public long MinQuantity { get; set; }
        public long MaxQuantity { get; set; }
        public double Weight { get; set; }
        public int GroupId { get; set; }
        public long MinLevel { get; set; }

    }
    public class SpawnSettings : ParentSettings<SpawnTable>
    {
        public override string Id { get; set; }
        public float MapSpawnChance { get; set; }
        public long MonsterLootSpawnTableId { get; set; }
    }
    public class SpawnTable : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public List<SpawnItem> Items { get; set; }
        public string Art { get; set; }

        public SpawnTable()
        {
            Items = new List<SpawnItem>();
        }

    }
    public class SpawnSettingsDto : ParentSettingsDto<SpawnSettings, SpawnTable>
    {
        public override List<SpawnTable> Children { get; set; }
        public override SpawnSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class SpawnSettingsLoader : ParentSettingsLoader<SpawnSettings, SpawnTable> { }

    public class SpawnSettingsMapper : ParentSettingsMapper<SpawnSettings, SpawnTable, SpawnSettingsDto> { }


    public class SpawnHelper : BaseEntityHelper<SpawnSettings, SpawnTable>
    {
        public override long HelperKey => EntityTypes.Spawn;
    }


}


