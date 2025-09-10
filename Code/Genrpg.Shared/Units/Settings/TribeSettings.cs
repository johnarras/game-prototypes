using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Units.Settings
{
    [MessagePackObject]
    public class TribeSettings : ParentSettings<TribeType>
    {
        [Key(0)] public override string Id { get; set; }

    }
    [MessagePackObject]
    public class TribeType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }

        [Key(8)] public List<SpawnItem> LootItems { get; set; } = new List<SpawnItem>();
        [Key(9)] public List<SpawnItem> InteractLootItems { get; set; } = new List<SpawnItem>();
        [Key(10)] public long LootCrafterTypeId { get; set; }

        [Key(11)] public bool HasRangedAttacks { get; set; }

        public class TribeSettingsDto : ParentSettingsDto<TribeSettings, TribeType> { }

        public class TribeSettingsLoasder : ParentSettingsLoader<TribeSettings, TribeType> { }

        public class TribeTypeSettingsMapper : ParentSettingsMapper<TribeSettings, TribeType, TribeSettingsDto> { }
    }
}
