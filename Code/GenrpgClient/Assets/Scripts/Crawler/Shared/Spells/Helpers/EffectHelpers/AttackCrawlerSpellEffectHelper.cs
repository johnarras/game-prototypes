using Genrpg.Shared.Entities.Constants;

namespace Assets.Scripts.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public class AttackCrawlerSpellEffectHelper : BaseDamageCrawlerSpellEffectHelper
    {
        public override long HelperKey => EntityTypes.Attack;
    }
}
