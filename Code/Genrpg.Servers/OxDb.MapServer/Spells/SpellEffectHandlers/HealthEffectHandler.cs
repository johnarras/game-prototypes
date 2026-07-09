using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Constants;
using OxDb.SharedGame.Spells.Settings.Effects;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Units.Constants;
using OxDb.SharedGame.Units.Entities;

namespace OxDb.MapServer.Spells.SpellEffectHandlers
{
    public abstract class HealthEffectHandler : BaseSpellEffectHandler
    {
        public override long HelperKey => -1;
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return true; }
        public override float GetTickLength() { return SpellConstants.DotTickSeconds; }

        public override bool HandleEffect(MapObject obj, ActiveSpellEffect eff)
        {
            if (!_objectManager.GetUnit(eff.TargetId, out Unit targ) || targ.HasFlag(UnitFlags.IsDead))
            {
                return false;
            }

            bool isCrit = false;
            long quantity = eff.CurrQuantity;
            if (obj.Rand.NextDouble() < eff.CritChance)
            {
                isCrit = true;
                quantity = (long)(quantity * eff.CritMult);
            }

            if (quantity < 0)
            {

                if (targ as Character != null)
                {
                    _spellService.ShowCombatText(targ, quantity.ToString(), CombatTextColors.Red, isCrit);
                }
                else
                {
                    int textColorId = eff.SpellId == 1 ? CombatTextColors.White : CombatTextColors.Yellow;

                    _spellService.ShowCombatText(targ, quantity.ToString(), textColorId, isCrit);

                }
            }
            else if (quantity > 0)
            {
                _spellService.ShowCombatText(targ, quantity.ToString(), CombatTextColors.Green, isCrit);
            }

            _statService.Add(targ, StatTypes.Health, UnitStatValOffsets.Curr, quantity);
            if (targ.Stats.Curr(StatTypes.Health) <= 0)
            {
                _unitService.CheckForDeath(targ, eff);
            }

            return true;
        }
    }
}


