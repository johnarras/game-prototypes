using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class RoleScalingInfoHelper : BaseInfoHelper<RoleScalingTypeSettings, RoleScalingType>
    {
        public override long Key => EntityTypes.RoleScaling;

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> startLines = base.GetInfoLines(entityId);

            RoleScalingType rsType = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).Get(entityId);

            if (rsType != null)
            {
                StatType statType = _gameData.Get<StatSettings>(_gs.ch).Get(rsType.ScalingStatTypeId);

                if (statType != null)
                {
                    startLines.Add("Abilities scale with the " + _infoService.CreateInfoLink(statType) + " Stat.");
                }
            }
            return startLines;
        }
    }
}
