using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Stats.Settings.Stats;
using System.Text;

namespace OxDb.SharedGame.Crawler.Info.SpellEffectHelpers
{
    public class StatSpellEffectHelper : BaseSpellEffectHelper
    {
        public override long HelperKey => EntityTypes.Stat;

        public override string ShowEffectInfo(CrawlerSpell spell, CrawlerSpellEffect effect)
        {
            StringBuilder sb = new StringBuilder();

            StatType statType = _gameData.Get<StatSettings>(_gs.ch).Get(effect.EntityId);

            if (statType != null)
            {
                sb.Append($"Adds 1 {_infoService.CreateInfoLink(statType)} per level of the caster to the target for the duration of combat (only largest buff counts).");
            }

            return sb.ToString();
        }
    }
}


