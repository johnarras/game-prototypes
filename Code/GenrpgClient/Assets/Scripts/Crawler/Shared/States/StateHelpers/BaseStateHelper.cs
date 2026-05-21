using Assets.Scripts.ClientEvents;
using Assets.Scripts.Core;
using Assets.Scripts.UI.Interfaces;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.Crawler.Combat.Services;
using OxDb.SharedGame.Crawler.Maps.Services;
using OxDb.SharedGame.Crawler.Options.Services;
using OxDb.SharedGame.Crawler.Spells.Services;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Stats.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers
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
            _dispatcher.Dispatch(new ShowInfoPanelArgs() { EntityTypeId = entityTypeId, EntityId = entityId });
        }

        virtual protected void ShowInfo(List<string> lines)
        {
            _dispatcher.Dispatch(new ShowInfoPanelArgs() { Lines = lines });
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


