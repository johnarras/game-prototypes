using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.Shared.Spells.Utils
{
    public class SpellUtils
    {
        public static bool IsValidTarget(Unit target, long casterFactionId, long targetTypeId)
        {
            if (targetTypeId == TargetTypes.Enemy)
            {
                if (target.FactionTypeId != casterFactionId)
                {
                    return true;
                }
            }


            if (targetTypeId == TargetTypes.Ally)
            {
                if (target.FactionTypeId == casterFactionId)
                {
                    return true;
                }
            }

            return false;
        }
        public static float GetResendDelay(bool isInstant)
        {
            return isInstant ? SpellConstants.ResendInstantDelaySec : SpellConstants.ResendProjectileDelaySec;
        }
    }
}


