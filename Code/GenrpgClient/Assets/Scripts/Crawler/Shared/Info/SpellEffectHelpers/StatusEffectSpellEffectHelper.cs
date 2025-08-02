using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.SpellEffectHelpers
{
    public class StatusEffectSpellEffectHelper : BaseSpellEffectHelper
    {
        public override long Key => EntityTypes.StatusEffect;

        public override string ShowEffectInfo(CrawlerSpell spell, CrawlerSpellEffect effect)
        {
            CrawlerCombatSettings settings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);
            StringBuilder sb = new StringBuilder();
            if (effect.MinQuantity < 0)
            {
                sb.Append($"Removes weakest status effect (1 + {settings.ExtraCureStatusEffectsRemovedPerTier} per tier) up to {GetRoleScalingText(spell,effect, " your ")} Tier.");
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
