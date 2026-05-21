using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.States.StateHelpers.Selection.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public interface ISpecialMagicHelper : ISetupDictionaryItem<long>
    {
        Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData, SelectSpellAction action,
            CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token);
    }
}


