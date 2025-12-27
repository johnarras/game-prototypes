using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Entities.Settings
{
    public class EntitySettings : ParentConstantListSettings<EntityType, EntityTypes>
    {
        public override string Id { get; set; }
    }

    public class EntityType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
    }

    public class EntitySettingsDto : ParentSettingsDto<EntitySettings, EntityType>
    {
        public override List<EntityType> Children { get; set; }
        public override EntitySettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class EntitySettingsLoader : ParentSettingsLoader<EntitySettings, EntityType> { }

    public class EntitySettingsMapper : ParentSettingsMapper<EntitySettings, EntityType, EntitySettingsDto> { }

    public class RandomEntityHelper : BaseEntityHelper<EntitySettings, EntityType>
    {
        public override long HelperKey => EntityTypes.RandomEntity;
    }
}


