using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.SpellCrafting.Constants;

namespace OxDb.SharedGame.SpellCrafting.SpellModifierHelpers
{
    public class DurationSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long HelperKey => SpellModifiers.Duration;

        public override double GetCostScale(MapObject obj, double value)
        {
            value = GetValidValue(obj, value);

            double scale = 1.0f;

            if (value > 0)
            {
                scale++;
            }
            if (value > 1)
            {
                scale += 0.9;
            }
            if (value > 2)
            {
                scale += 0.8;
            }
            if (value > 3)
            {
                scale += (value - 3) * 0.7;
            }

            return scale;
        }
    }
}


