using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.SpellCrafting.Constants;

namespace OxDb.SharedGame.SpellCrafting.SpellModifierHelpers
{
    public class RangeSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long HelperKey => SpellModifiers.Range;

        // Linear in range, max at 3x at 45 units so 
        public override double GetCostScale(MapObject obj, double value)
        {
            value = GetValidValue(obj, value);

            return 1.0f + value * 0.05;
        }
    }
}


