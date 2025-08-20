using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.UI.Constants;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class CrawlerHUDButtons : BaseBehaviour
    {

        private ICrawlerService _crawlerService = null;


        public GButton MapButton;
        public GButton SafetyButton;
        public GButton InfoButton;
        public GButton MainMenuButton;
        public GButton CastButton;
        public GButton QuestLogButton;
        public GButton PartyOrderButton;
        public GButton ResetGameButton;

        public override void Init()
        {
            _uiService.SetButton(MapButton, GetType().Name, ClickMapScreen);
            _uiService.SetButton(MapButton, GetType().Name, ClickMapScreen);
            _uiService.SetButton(SafetyButton, GetType().Name, ClickSafety);
            _uiService.SetButton(InfoButton, GetType().Name, ClickInfo);
            _uiService.SetButton(MainMenuButton, GetType().Name, ClickMainMenu);
            _uiService.SetButton(CastButton, GetType().Name, ClickCast);
            _uiService.SetButton(QuestLogButton, GetType().Name, ClickQuestLog);
            _uiService.SetButton(PartyOrderButton, GetType().Name, ClickPartyOrder);
            _uiService.SetButton(ResetGameButton, GetType().Name, ResetGame);
        }

        private void ClickMapScreen()
        {
            PartyData party = _crawlerService.GetParty();

            if (party.Buffs.Get(PartyBuffs.Mapping) == 0)
            {
                _dispatcher.Dispatch(new ShowFloatingText("You can only look at maps when mapping is active.", EFloatingTextArt.Error));
                return;
            }

            _screenService.Open(ScreenNames.CrawlerMap);
        }

        private void ClickMainMenu()
        {
            _screenService.Open(ScreenNames.CrawlerMainMenu);
        }

        private void ClickInfo()
        {
            _screenService.Open(ScreenNames.CrawlerInfo);
        }

        private void ClickSafety()
        {
            if (_crawlerService.GetState() == ECrawlerStates.ExploreWorld)
            {
                _crawlerService.ChangeState(ECrawlerStates.ReturnToSafety, GetToken());
            }
        }

        private void ClickCast()
        {
            if (_crawlerService.GetState() == ECrawlerStates.ExploreWorld)
            {
                _crawlerService.ChangeState(ECrawlerStates.SelectAlly, GetToken());
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

        private void ResetGame()
        {
            _initClient.FullResetGame();
        }
    }
}
