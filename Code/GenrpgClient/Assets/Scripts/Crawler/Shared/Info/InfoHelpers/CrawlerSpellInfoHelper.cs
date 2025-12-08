using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Info.InfoHelpers;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Spells.Settings.Targets;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Info.Helpers
{
    public class CrawlerSpellInfoHelper : BaseInfoHelper<CrawlerSpellSettings, CrawlerSpell>
    {

        private ICrawlerSpellService _spellService = null;

        public override long HelperKey => EntityTypes.CrawlerSpell;

        protected override bool IsValidInfoChild(CrawlerSpell child)
        {
            return child.IdKey < 1000;
        }

        public override List<string> GetInfoLines(long entityId)
        {
            CrawlerSpell spell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).Get(entityId);

            List<string> allLines = new List<string>();

            RoleScalingType scalingType = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).Get(spell.RoleScalingTypeId);

            allLines.Add(_infoService.CreateHeaderLine(spell.Name));
            allLines.Add("Tier " + spell.RoleScalingTier + " " + _infoService.CreateInfoLink(scalingType) + " Scaling");
            if (spell.PowerPerLevel == 0)
            {
                allLines.Add("Cost: " + spell.PowerCost);
            }
            else
            {
                allLines.Add("Cost: " + spell.PowerCost + " +" + spell.PowerPerLevel + "/Tier");
            }

            TargetType ttype = _gameData.Get<TargetTypeSettings>(_gs.ch).Get(spell.TargetTypeId);

            allLines.Add("Target: " + ttype.Name + " " + ttype.Desc);

            CombatAction action = _gameData.Get<CombatActionSettings>(_gs.ch).Get(spell.CombatActionId);

            if (action.BaseBonusHits > 0)
            {
                allLines.Add($"When used, this ability gains an extra {action.BaseBonusHits} hit" + (action.BaseBonusHits == 1 ? "" : "s") +
                    " beyond the party member's role scaling tier.");
            }

            allLines.Add("Desc: " + spell.Desc);

            EntitySettings entitySettings = _gameData.Get<EntitySettings>(_gs.ch);
            ElementTypeSettings elementSettings = _gameData.Get<ElementTypeSettings>(_gs.ch);

            foreach (CrawlerSpellEffect effect in spell.Effects)
            {
                string effectText = _infoService.GetEffectText(spell, effect);

                if (!string.IsNullOrEmpty(effectText))
                {
                    allLines.Add("Effect:  " + effectText);
                }
            }
            allLines.Add("\n" + _spellService.RolesThatCanCastString(spell.IdKey));

            return allLines;
        }
    }
}
