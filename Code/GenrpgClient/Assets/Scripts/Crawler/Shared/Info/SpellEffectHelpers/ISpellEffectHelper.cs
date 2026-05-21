using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Spells.Settings;

namespace OxDb.SharedGame.Crawler.Info.EffectHelpers
{
    public interface ISpellEffectHelper : ISetupDictionaryItem<long>
    {
        string ShowEffectInfo(CrawlerSpell spell, CrawlerSpellEffect effect);
    }
}


