using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Spells.Settings.Effects;
using OxDb.SharedGame.Spells.Settings.Elements;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;

namespace OxDb.MapServer.Spells.SpellEffectHandlers
{
    public class UnitEffectHandler : BaseSpellEffectHandler
    {
        public override long HelperKey => EntityTypes.Unit;
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return false; }

        public override List<ActiveSpellEffect> CreateEffects(IRandom rand, SpellHit hitData)
        {
            List<ActiveSpellEffect> retval = new List<ActiveSpellEffect>();

            ElementSkill elemSkill = hitData.ElementType.GetSkill(hitData.SkillType.IdKey);

            if (elemSkill == null || elemSkill.OverrideEntityTypeId != EntityTypes.Unit)
            {
                return retval;
            }

            ActiveSpellEffect eff = new ActiveSpellEffect(hitData);
            eff.EntityTypeId = EntityTypes.Unit;
            eff.EntityId = elemSkill.OverrideEntityId;
            eff.Quantity = elemSkill.ScalePct;


            retval.Add(eff);
            return retval;
        }

        public override bool HandleEffect(IRandom rand, ActiveSpellEffect eff)
        {

            if (!_objectManager.GetUnit(eff.TargetId, out Unit targ))
            {
                return false;
            }

            MyPointF pos = targ.GetPos();
            pos.Z += 2;

            int statPct = (int)eff.Quantity;

            UnitGenData unitGenData = new UnitGenData()
            {
                UnitTypeId = eff.EntityId,
                Level = eff.Level,
                FactionTypeId = targ.FactionTypeId,
                Pos = pos,
                StatPct = statPct,
            };

            return true;
        }
    }
}


