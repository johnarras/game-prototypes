using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Combat.Settings;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.UnitEffects.Settings;
using System.Text;

namespace OxDb.SharedGame.Crawler.Info.SpellEffectHelpers
{
    public class StatusEffectSpellEffectHelper : BaseSpellEffectHelper
    {
        public override long HelperKey => EntityTypes.StatusEffect;

        public override string ShowEffectInfo(CrawlerSpell spell, CrawlerSpellEffect effect)
        {
            CrawlerCombatSettings settings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);
            StringBuilder sb = new StringBuilder();
            if (effect.WeaponDamageScale < 0)
            {
                sb.Append($"Removes weakest status effect (1 + {settings.ExtraCureStatusEffectsRemovedPerTier} per tier) up to {GetRoleScalingText(spell, effect, " your ")} Tier.");
            }
            else
            {
                StatusEffect statusEffect = _gameData.Get<StatusEffectSettings>(_gs.ch).Get(effect.EntityId);

                if (statusEffect != null)
                {
                    sb.Append($"Applies the {_infoService.CreateInfoLink(statusEffect)} to the target.");
                }
            }

            return sb.ToString();
        }
    }
}


