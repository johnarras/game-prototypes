using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Roles.Settings
{
    public class RoleScalingTypeSettings : ParentConstantListSettings<RoleScalingType, RoleScalingTypes>
    {
        public override string Id { get; set; }
        public long PointsPerLevel { get; set; }

    }

    public class RoleScalingType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public long ScalingStatTypeId { get; set; }
        public long ScalingEquipSlotId { get; set; }


    }


    public class RoleScalingTypeSettingsDto : ParentSettingsDto<RoleScalingTypeSettings, RoleScalingType>
    {
        public override List<RoleScalingType> Children { get; set; }
        public override RoleScalingTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class RoleScalingTypeSettingsLoader : ParentSettingsLoader<RoleScalingTypeSettings, RoleScalingType> { }

    public class RoleScalingTypeSettingsMapper : ParentSettingsMapper<RoleScalingTypeSettings, RoleScalingType, RoleScalingTypeSettingsDto> { }


    public class RoleScalingHelper : BaseEntityHelper<RoleScalingTypeSettings, RoleScalingType>
    {
        public override long HelperKey => EntityTypes.RoleScaling;
    }

}


