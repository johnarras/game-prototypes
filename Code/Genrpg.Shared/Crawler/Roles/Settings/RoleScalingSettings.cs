using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.Crawler.Roles.Constants;

namespace Genrpg.Shared.Crawler.Roles.Settings
{
    [MessagePackObject]
    public class RoleScalingTypeSettings : ParentConstantListSettings<RoleScalingType,RoleScalingTypes>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long PointsPerLevel { get; set; }

    }

    [MessagePackObject]
    public class RoleScalingType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string NameId { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string AtlasPrefix { get; set; }
        [Key(7)] public string Icon { get; set; }
        [Key(8)] public string Art { get; set; }
        [Key(9)] public long ScalingStatTypeId { get; set; }
        [Key(10)] public long ScalingEquipSlotId { get; set; }


    }


    public class RoleScalingTypeSettingsDto : ParentSettingsDto<RoleScalingTypeSettings, RoleScalingType> { }
    public class RoleScalingTypeSettingsLoader : ParentSettingsLoader<RoleScalingTypeSettings, RoleScalingType> { }

    public class RoleScalingTypeSettingsMapper : ParentSettingsMapper<RoleScalingTypeSettings, RoleScalingType, RoleScalingTypeSettingsDto> { }


    public class RoleScalingHelper : BaseEntityHelper<RoleScalingTypeSettings,RoleScalingType>
    {
        public override long Key => EntityTypes.RoleScaling;
    }

}
