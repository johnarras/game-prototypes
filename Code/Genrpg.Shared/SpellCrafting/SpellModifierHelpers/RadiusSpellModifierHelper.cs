using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Constants;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public class RadiusSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long HelperKey => SpellModifiers.Radius;

        const double _radiusDiv = 3;

        // Square of radius/3?
        public override double GetCostScale(MapObject obj, double value)
        {
            value = GetValidValue(obj, value);

            value /= _radiusDiv;

            return 1 + value * value;

        }
    }
}


