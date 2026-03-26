using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Constants;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
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


