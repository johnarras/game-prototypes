using OxDb.SharedCore.Entities.Constants;

namespace OxDb.Client.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public class AttackCrawlerSpellEffectHelper : BaseDamageCrawlerSpellEffectHelper
    {
        public override long HelperKey => EntityTypes.Attack;
    }
}


