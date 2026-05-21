using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.SpellCrafting.Constants;

namespace OxDb.SharedGame.SpellCrafting.SpellModifierHelpers
{
    public class ExtraTargetsSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long HelperKey => SpellModifiers.ExtraTargets;


        const double _extraTargetMult = 0.9f;

        // Scaling is just linear, but smaller percent. 0.9?

        public override double GetCostScale(MapObject obj, double value)
        {
            value = GetValidValue(obj, value);

            return 1.0 + value * _extraTargetMult;
        }
    }
}


