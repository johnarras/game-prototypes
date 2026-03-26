using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Constants;
using System;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public class MaxChargesSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long HelperKey => SpellModifiers.MaxCharges;

        public override double GetCostScale(MapObject obj, double value)
        {
            value = GetValidValue(obj, value);

            if (value < 1)
            {
                return 1.0f;
            }

            return 1.0f + Math.Sqrt(value);
        }
    }
}


