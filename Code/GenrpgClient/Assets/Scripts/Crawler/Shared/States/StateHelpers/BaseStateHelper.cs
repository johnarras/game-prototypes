using Assets.Scripts.ClientEvents;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers
{
    public abstract class BaseStateHelper : IStateHelper
    {

        protected ICrawlerService _crawlerService;
        protected ICrawlerStatService _statService;
        protected ICrawlerCombatService _combatService;
        protected ICrawlerSpellService _crawlerSpellService;
        protected ILogService _logService;
        protected IRepositoryService _repoService;
        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected IClientRandom _rand;
        protected ICrawlerWorldService _worldService;
        protected IDispatcher _dispatcher;
        protected ITextService _textService;

        public abstract ECrawlerStates Key { get; }
        public abstract Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token);

        public virtual bool IsTopLevelState() { return false; }
        public virtual long TriggerBuildingId() { return 0; }
        public virtual long TriggerDetailEntityTypeId() { return 0; }
        protected virtual bool OnlyUseBGImage() { return false; }
        public virtual bool HideBigPanels() { return false; }
        public virtual bool ShouldDispatchClickKeys() { return false; }

        protected virtual CrawlerStateData CreateStateData()
        {
            return new CrawlerStateData(Key)
            {
                BGImageOnly = OnlyUseBGImage(),
            };
        }

        virtual protected void ShowInfo(long entityTypeId, long entityId)
        {
            _dispatcher.Dispatch(new ShowInfoPanelEvent() { EntityTypeId = entityTypeId, EntityId = entityId });
        }

        virtual protected void ShowInfo(List<string> lines)
        {
            _dispatcher.Dispatch(new ShowInfoPanelEvent() { Lines = lines });
        }

        virtual protected void AddSpaceAction(CrawlerStateData stateData, ECrawlerStates nextState = ECrawlerStates.ExploreWorld, object extraData = null)
        {
            stateData.Actions.Add(new CrawlerStateAction($"\n\nPress {_textService.HighlightText("Space")} to continue...", CharCodes.Space, nextState,
                extraData: extraData));
        }
    }
}
