using Assets.Scripts.ClientEvents;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Options.Services;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Genrpg.Shared.Crawler.States.StateHelpers
{
    public abstract class BaseStateHelper : IStateHelper
    {

        protected ICrawlerService _crawlerService = null;
        protected ICrawlerStatService _statService = null;
        protected ICrawlerCombatService _combatService = null;
        protected ICrawlerSpellService _crawlerSpellService = null;
        protected ILogService _logService = null;
        protected IRepositoryService _repoService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IClientRandom _rand = null;
        protected ICrawlerWorldService _worldService = null;
        protected IDispatcher _dispatcher = null;
        protected ITextService _textService = null;
        protected ICrawlerOptionsService _optionsService = null;
        protected IInputService _inputService = null;

        public abstract ECrawlerStates HelperKey { get; }
        public abstract Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token);

        public virtual bool IsTopLevelState() { return false; }
        public virtual long TriggerBuildingId() { return 0; }
        public virtual long TriggerDetailEntityTypeId() { return 0; }
        protected virtual bool OnlyUseBGImage() { return false; }
        public virtual bool HideBigPanels() { return false; }
        public virtual bool ShouldDispatchClickKeys() { return false; }

        protected virtual CrawlerStateData CreateStateData()
        {
            return new CrawlerStateData(HelperKey)
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
            stateData.Actions.Add(new CrawlerStateAction($"\n\nPress {_textService.HighlightText("Space")} to continue...", Key.Space, nextState,
                extraData: extraData));
        }

        protected virtual Key FromChar(char c)
        {
            return _inputService.FromChar(c);
        }
    }
}


