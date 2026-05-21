using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Achievements.Constants;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Spells.Interfaces;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Spells.Settings.Effects;
using OxDb.SharedGame.Units.Constants;
using OxDb.SharedGame.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.MapServer.Spells.SpellEffectHandlers
{
    public class DamageEffectHandler : HealthEffectHandler
    {


        public override long HelperKey => EntityTypes.Damage;
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return true; }


        public override List<ActiveSpellEffect> CreateEffects(IRandom rand, SpellHit hitData)
        {
            ActiveSpellEffect eff = new ActiveSpellEffect(hitData);
            eff.EntityTypeId = EntityTypes.Damage;
            eff.Quantity = hitData.BaseQuantity;
            return new List<ActiveSpellEffect>() { eff };
        }

        public override bool HandleEffect(IRandom rand, ActiveSpellEffect eff)
        {
            if (!_objectManager.GetUnit(eff.TargetId, out Unit targ) || targ.HasFlag(UnitFlags.IsDead))
            {
                return false;
            }

            long startAmount = eff.Quantity;
            long amount = eff.Quantity;

            int variancePct = 20;

            amount = RandUtils.LongRange(startAmount * (100 - variancePct) / 100,
                startAmount * (100 + variancePct) / 100, rand);

            long absorbAmount = 0;
            bool isImmune = targ.IsFullImmune();

            if (isImmune)
            {
                amount = 0;
            }
            if (amount != 0 && _objectManager.GetChar(eff.CasterId, out Character ch))
            {

                _achievementService.UpdateAchievement(ch, AchievementTypes.TotalDamage, amount);
                _achievementService.UpdateAchievement(ch, AchievementTypes.MaxDamage, amount);
            }

            amount = -amount;

            if (targ.Effects == null)
            {
                targ.Effects = new List<IDisplayEffect>();
            }
            List<IDisplayEffect> shields = targ.Effects.Where(x => x.EntityTypeId == EntityTypes.Shield).ToList();

            foreach (ActiveSpellEffect shield in shields)
            {
                long currAbsorb = Math.Min(-amount, shield.Quantity);
                amount += currAbsorb;
                shield.Quantity -= currAbsorb;
                if (shield.Quantity <= 0)
                {
                    targ.Effects.Remove(shield);
                    shield.SetCancelled(true);
                }
                absorbAmount += currAbsorb;
                if (amount <= 0)
                {
                    break;
                }
            }
            eff.CurrQuantity = amount;
            return base.HandleEffect(rand, eff);
        }
    }
}


