using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Combat.Services;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Spells.Constants;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Factions.Constants;
using OxDb.SharedGame.Spells.Settings.Elements;
using OxDb.SharedGame.UnitEffects.Settings;
using OxDb.SharedGame.Units.Settings;
using System.Collections.Generic;
using System.Text;

namespace OxDb.SharedGame.Crawler.Info.InfoHelpers
{
    public class UnitInfoHelper : BaseInfoHelper<UnitTypeSettings, UnitType>
    {

        private ICrawlerCombatService _combatService = null;
        private ICrawlerService _crawlerService = null;


        public override long HelperKey => EntityTypes.Unit;

        public override List<string> GetInfoLines(long entityId)
        {

            UnitType unitType = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(entityId);
            List<string> lines = new List<string>();


            lines.Add(_infoService.CreateHeaderLine(unitType.Name, false));
            lines.Add(unitType.Desc ?? "");

            FullMonsterStats stats = _combatService.GetFullMonsterStats(_crawlerService.GetParty(), unitType, FactionTypes.Player, 1000, false);

            lines.Add("Min Range: " + stats.Range);

            if (stats.IsGuardian)
            {
                lines.Add("GUARDIAN");
            }

            CrawlerSpellSettings spellSettings = _gameData.Get<CrawlerSpellSettings>(_gs.ch);
            ElementTypeSettings elementSettings = _gameData.Get<ElementTypeSettings>(_gs.ch);
            StatusEffectSettings statusSettings = _gameData.Get<StatusEffectSettings>(_gs.ch);

            lines.Add("Abilities:");


            foreach (Effect unitSpell in stats.Spells)
            {
                CrawlerSpell spell = spellSettings.Get(unitSpell.EntityId);

                if (spell != null && spell.IdKey > CrawlerSpells.ShootId)
                {
                    lines.Add(" " + _infoService.CreateInfoLink(spell) + ": " + spell.Desc);
                }
            }

            StringBuilder sb = new StringBuilder();

            foreach (ElementType etype in elementSettings.GetData())
            {
                if (FlagUtils.MatchesAnyBits(stats.ResistBits, etype.IdKey))
                {
                    sb.Append(_infoService.CreateInfoLink(etype) + " ");
                }
            }

            if (sb.Length > 0)
            {
                lines.Add("Resistances: " + sb.ToString());
            }

            sb.Clear();

            foreach (ElementType etype in elementSettings.GetData())
            {
                if (FlagUtils.MatchesAnyBits(stats.VulnBits, etype.IdKey))
                {
                    sb.Append(_infoService.CreateInfoLink(etype) + " ");
                }
            }

            if (sb.Length > 0)
            {
                lines.Add("Vulnerabilities: " + sb.ToString());
            }

            sb.Clear();

            foreach (FullEffect applyEffect in stats.ApplyEffects)
            {
                StatusEffect statusEffect = statusSettings.Get(applyEffect.Effect.EntityId);

                if (statusEffect != null)
                {
                    sb.Append(_infoService.CreateInfoLink(statusEffect) + " ");
                }
            }

            if (sb.Length > 0)
            {
                lines.Add("On Hit Effects: " + sb.ToString());
            }



            return lines;
        }

    }
}


