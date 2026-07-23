using OxDb.Client.FloatingText.ClientEvents;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.StateHelpers.Selection.Entities;
using OxDb.SharedGame.Spells.Constants;
using OxDb.SharedGame.Spells.Settings.SpecialMagic;

using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public class IdentifyItemSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long HelperKey => SpecialMagics.IdentifyItem;

        public override async Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData, SelectSpellAction action,
            CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {
            SpecialMagic magic = _gameData.Get<SpecialMagicSettings>(null).Get(effect.EntityId);

            _dispatcher.Dispatch(new ShowFloatingText("Special spell: " + magic?.Name ?? "Effect " + effect.EntityId));

            await Task.CompletedTask;
            return new CrawlerStateData(ECrawlerStates.ExploreWorld, true);
        }
    }
}


