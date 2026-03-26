using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Crawler.Buffs.Services;
using Assets.Scripts.Crawler.Shared.States.StateHelpers.Selection;
using Assets.Scripts.Crawler.Tilemaps;
using Assets.Scripts.FloatingText.ClientEvents;
using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.Crawler.Options.Constants;
using Genrpg.Shared.Crawler.Options.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.UI.Constants;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class CrawlerHUDButtons : BaseBehaviour
    {

        private ICrawlerService _crawlerService = null;
        private IAwaitableService _awaitableService = null;
        private IBuffService _buffService = null;
        private ICrawlerOptionsService _optionsService = null;
        private IClientAppService _appService = null;
        protected IScreenService _screenService = null;

        public GButton MapButton;
        public GButton SafetyButton;
        public GButton InfoButton;
        public GButton CastButton;
        public GButton QuestLogButton;
        public GButton PartyOrderButton;
        public GButton CastPartyBuffsButton;
        public GButton UseItemButton;
        public GButton CampingButton;
        public GButton SnapshotButton;
        public GButton OptionsButton;

        public override void Init()
        {
            _uiService.SetButton(MapButton, name, ClickMapScreen);
            _uiService.SetButton(SafetyButton, name, ClickSafety);
            _uiService.SetButton(InfoButton, name, ClickInfo);
            _uiService.SetButton(CampingButton, name, ClickCamping);
            _uiService.SetButton(CastButton, name, ClickCastSpell);
            _uiService.SetButton(QuestLogButton, name, ClickQuestLog);
            _uiService.SetButton(PartyOrderButton, name, ClickPartyOrder);
            _uiService.SetButton(CastPartyBuffsButton, name, CastAllPartyBuffs);
            _uiService.SetButton(UseItemButton, name, ClickUseItem);
            _uiService.SetButton(SnapshotButton, name, ClickTakeSnapshot);
            _uiService.SetButton(OptionsButton, name, ClickOptions);

            PartyData party = _crawlerService.GetParty();
            if (!_optionsService.HasOption(party, CrawlerOptions.Camping))
            {
                _clientEntityService.SetActive(CampingButton.gameObject, false);
            }
        }

        private void ClickMapScreen()
        {
            PartyData party = _crawlerService.GetParty();

            if (CrawlerTilemap.RequireMapping && party.Buffs[PartyBuffs.Mapping] == 0)
            {
                _dispatcher.Dispatch(new ShowFloatingText("You can only look at maps when mapping is active.", EFloatingTextArt.Error));
                return;
            }

            if (_screenService.GetScreen(ScreenNames.CrawlerMap) == null)
            {
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.CrawlerMap));
            }
            else
            {
                _dispatcher.Dispatch(new CloseScreen(ScreenNames.CrawlerMap));
            }
        }

        private void ClickCamping()
        {
            if (_crawlerService.GetState() == ECrawlerStates.ExploreWorld)
            {
                _crawlerService.ChangeState(ECrawlerStates.Camping, GetToken());
            }
        }

        private void ClickInfo()
        {
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.CrawlerInfo));
        }
        private void ClickOptions()
        {
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.ClientOptions));
        }

        private void ClickSafety()
        {
            if (_crawlerService.GetState() == ECrawlerStates.ExploreWorld)
            {
                _crawlerService.ChangeState(ECrawlerStates.ReturnToSafety, GetToken());
            }
        }

        private void ClickCastSpell()
        {
            if (_crawlerService.GetState() == ECrawlerStates.ExploreWorld)
            {
                _crawlerService.ChangeState(ECrawlerStates.SelectAlly, GetToken());
            }
        }

        private void ClickUseItem()
        {
            if (_crawlerService.GetState() == ECrawlerStates.ExploreWorld)
            {
                SelectUsableItemArgs args = new SelectUsableItemArgs()
                {
                    MemberId = null,
                    NextState = ECrawlerStates.OnSelectSpell,
                    ReturnState = ECrawlerStates.ExploreWorld,
                };

                _crawlerService.ChangeState(ECrawlerStates.SelectUsableItem, GetToken(), args);
            }
        }

        private void ClickQuestLog()
        {
            if (_crawlerService.GetState() == ECrawlerStates.ExploreWorld)
            {
                _crawlerService.ChangeState(ECrawlerStates.QuestLog, GetToken());
            }
        }

        private void ClickPartyOrder()
        {
            if (_crawlerService.GetState() == ECrawlerStates.ExploreWorld)
            {
                _crawlerService.ChangeState(ECrawlerStates.PartyOrder, GetToken(), null, ECrawlerStates.ExploreWorld);
            }
        }

        private void CastAllPartyBuffs()
        {
            _awaitableService.ForgetTask(_buffService.CastAllPartyBuffs(_crawlerService.GetParty(), GetToken()));
        }

        private void ClickTakeSnapshot()
        {
            _appService.TakeMemorySnapshot();
        }
    }
}


