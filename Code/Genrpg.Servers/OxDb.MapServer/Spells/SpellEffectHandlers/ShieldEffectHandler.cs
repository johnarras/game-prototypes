using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Spells.Settings.Effects;
using System.Collections.Generic;

namespace OxDb.MapServer.Spells.SpellEffectHandlers
{
    public class ShieldEffectHandler : BaseSpellEffectHandler
    {
        public override long HelperKey => EntityTypes.Shield;
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return true; }

        public override List<ActiveSpellEffect> CreateEffects(MapObject obj, SpellHit hitData)
        {
            ActiveSpellEffect eff = new ActiveSpellEffect(hitData);
            eff.EntityTypeId = EntityTypes.Shield;
            eff.EntityId = 0;
            eff.Quantity = hitData.BaseQuantity;
            return new List<ActiveSpellEffect>() { eff };
        }

        public override bool HandleEffect(MapObject obj, ActiveSpellEffect eff)
        {
            return true;
        }
    }
}


