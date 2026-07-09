using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Spells.Settings.Effects;
using System.Collections.Generic;

namespace OxDb.MapServer.Spells.SpellEffectHandlers
{
    public class SpellEffectHandler : BaseSpellEffectHandler
    {
        public override long HelperKey => EntityTypes.Spell;
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return false; }

        public override List<ActiveSpellEffect> CreateEffects(MapObject obj, SpellHit hitData)
        {
            // Used for special spells that do unique things.

            return new List<ActiveSpellEffect>();


        }

        public override bool HandleEffect(MapObject obj, ActiveSpellEffect eff)
        {
            return true;
        }
    }
}


