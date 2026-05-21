using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.SpellCrafting.Constants;

namespace OxDb.SharedGame.SpellCrafting.SpellModifierHelpers
{
    public class ScaleSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long HelperKey => SpellModifiers.Scale;

        public override double GetCostScale(MapObject obj, double value)
        {
            value = GetValidValue(obj, value);

            return value / 100.0f;

        }
    }
}


