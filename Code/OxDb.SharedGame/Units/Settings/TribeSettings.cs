using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Spawns.Settings;
using System.Collections.Generic;

namespace OxDb.SharedGame.Units.Settings
{
    public class TribeSettings : ParentSettings<TribeType>
    {
        public override string Id { get; set; }

    }
    public class TribeType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public List<SpawnItem> LootItems { get; set; } = new List<SpawnItem>();
        public List<SpawnItem> InteractLootItems { get; set; } = new List<SpawnItem>();
        public long LootCrafterTypeId { get; set; }

        public bool HasRangedAttacks { get; set; }
    }

    public class TribeSettingsDto : ParentSettingsDto<TribeSettings, TribeType>
    {
        public override List<TribeType> Children { get; set; }
        public override TribeSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TribeSettingsLoasder : ParentSettingsLoader<TribeSettings, TribeType> { }

    public class TribeTypeSettingsMapper : ParentSettingsMapper<TribeSettings, TribeType, TribeSettingsDto> { }
}


