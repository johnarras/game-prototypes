using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.Crawler.Combat.Services;
using OxDb.SharedGame.Crawler.Maps.Services;
using OxDb.SharedGame.Crawler.Roles.Services;
using OxDb.SharedGame.Crawler.Spells.Services;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.States.StateHelpers.Selection.Entities;
using System.Threading;
using System.Threading.Tasks;


namespace OxDb.SharedGame.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public abstract class BaseSpecialMagicHelper : ISpecialMagicHelper
    {
        protected IDispatcher _dispatcher;
        protected IGameData _gameData;
        protected ICrawlerService _crawlerService = null;
        protected ICrawlerCombatService _combatService = null;
        protected ICrawlerMapService _mapService = null;
        protected ICrawlerWorldService _worldService = null;
        protected ILogService _logService = null;
        protected ICrawlerSpellService _spellService = null;
        protected ITextService _textService = null;
        protected IRoleService _roleService = null;

        public abstract long HelperKey { get; }

        public abstract Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData, SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect,
            CancellationToken token);
    }
}


