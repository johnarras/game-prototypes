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
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.UserCoins.Settings;

namespace Genrpg.Shared.Entities.Settings
{
    [MessagePackObject]
    public class EntitySettings : ParentConstantListSettings<EntityType,EntityTypes>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class EntityType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
    }

    public class EntitySettingsDto : ParentSettingsDto<EntitySettings, EntityType> { }

    public class EntitySettingsLoader : ParentSettingsLoader<EntitySettings, EntityType> { }

    public class EntitySettingsMapper : ParentSettingsMapper<EntitySettings, EntityType, EntitySettingsDto> { }

    public class RandomEntityHelper : BaseEntityHelper<EntitySettings, EntityType>
    {
        public override long Key => EntityTypes.RandomEntity;
    }
}
