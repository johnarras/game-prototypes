using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.SpellCrafting.Constants;

namespace OxDb.SharedGame.SpellCrafting.SpellModifierHelpers
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


