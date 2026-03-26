using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Constants;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public class CastTimeSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long HelperKey => SpellModifiers.CastTime;



        public override double GetCostScale(MapObject obj, double value)
        {
            value = GetValidValue(obj, value);

            return MathUtil.Clamp(0.25, 1.0f - value * 0.1f, 1);
        }
    }
}


