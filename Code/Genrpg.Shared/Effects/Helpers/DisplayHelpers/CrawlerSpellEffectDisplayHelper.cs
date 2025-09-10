using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Effects.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.PlayerFiltering.Interfaces;

namespace Genrpg.Shared.Effects.Helpers.DisplayHelpers
{
    public class CrawlerSpellEffectDisplayHelper : BaseEffectDisplayHelper
    {
        public override long Key => EntityTypes.CrawlerSpell;

        public override string DisplayEffect(IFilteredObject obj, IEffect effect)
        {
            CrawlerSpell spell = _gameData.Get<CrawlerSpellSettings>(null).Get(effect.EntityId);
            if (spell != null)
            {
                return "Casts " + spell.Name + " (L" + effect.Quantity + ")";
            }

            return null;
        }
    }
}
