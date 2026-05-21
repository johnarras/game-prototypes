using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedGame.Crawler.Spells.Settings;

namespace OxDb.SharedGame.Effects.Helpers.DisplayHelpers
{
    public class CrawlerSpellEffectDisplayHelper : BaseEffectDisplayHelper
    {
        public override long HelperKey => EntityTypes.CrawlerSpell;

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


