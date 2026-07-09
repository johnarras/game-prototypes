using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Achievements.Constants;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Spells.Settings.Effects;
using OxDb.SharedGame.Units.Constants;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;

namespace OxDb.MapServer.Spells.SpellEffectHandlers
{
    public class HealingEffectHandler : HealthEffectHandler
    {
        public override long HelperKey => EntityTypes.Healing;
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return true; }


        public override List<ActiveSpellEffect> CreateEffects(MapObject obj, SpellHit hitData)
        {
            ActiveSpellEffect eff = new ActiveSpellEffect(hitData);
            eff.EntityTypeId = EntityTypes.Healing;
            eff.Quantity = hitData.BaseQuantity;
            return new List<ActiveSpellEffect>() { eff };
        }

        public override bool HandleEffect(MapObject obj, ActiveSpellEffect eff)
        {
            if (!_objectManager.GetUnit(eff.TargetId, out Unit targ) || targ.HasFlag(UnitFlags.IsDead))
            {
                eff.SetCancelled(true);
                return false;
            }

            eff.CurrQuantity = eff.Quantity;

            int variancePct = 20;
            eff.CurrQuantity = RandUtils.LongRange(eff.Quantity * (100 - variancePct) / 100,
                eff.Quantity * (100 + variancePct) / 100, obj.Rand);

            if (eff.Quantity != 0 && _objectManager.GetChar(eff.CasterId, out Character ch))
            {
                _achievementService.UpdateAchievement(ch, AchievementTypes.TotalHealing, eff.Quantity);
                _achievementService.UpdateAchievement(ch, AchievementTypes.MaxHealing, eff.Quantity);
            }


            return base.HandleEffect(obj, eff);
        }
    }
}


