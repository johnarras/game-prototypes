using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Interfaces;
using Assets.Scripts.Core;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
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


