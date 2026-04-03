using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Crawler.Info.EffectHelpers
{
    public interface ISpellEffectHelper : ISetupDictionaryItem<long>
    {
        string ShowEffectInfo(CrawlerSpell spell, CrawlerSpellEffect effect);
    }
}


