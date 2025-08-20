using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Entities.Constants;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class RoleScalingInfoHelper : BaseInfoHelper<RoleScalingTypeSettings, RoleScalingType>
    {
        public override long Key => EntityTypes.RoleScaling;
    }
}
