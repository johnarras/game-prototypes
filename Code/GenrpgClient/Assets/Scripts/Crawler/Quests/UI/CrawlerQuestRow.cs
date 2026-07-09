using Assets.Scripts.Crawler.Maps.Services;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Quests.Entities;
using OxDb.SharedGame.Crawler.Quests.Services;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Quests.UI
{
    public class CrawlerQuestRow : BaseBehaviour
    {

        private ICrawlerService _crawlerService = null;
        private ICrawlerQuestService _questService = null;
        private ICrawlerWorldService _worldService = null;

        public GImage IsActiveImage;
        public GText Text;
        public GButton Button;

        private FullQuest _fullQuest = null;
        private bool _isActiveQuest = false;

        public void SetData(FullQuest fullQuest)
        {
            _fullQuest = fullQuest;
            _uiService.SetButton(Button, name,
                () =>
                {
                    _crawlerService.ChangeState(ECrawlerStates.QuestDetail, GetToken(), fullQuest);
                });
            UpdateData();
        }

        public long GetQuestId()
        {
            return _fullQuest?.Quest?.IdKey ?? 0;
        }

        public void UpdateData()
        {
            _ = ShowDataAsync(GetToken());
        }

        public bool IsActiveQuest()
        {
            return _isActiveQuest;
        }

        private async ValueTask ShowDataAsync(CancellationToken token)
        {
            PartyData party = _crawlerService.GetParty();
            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            string questStatus = await _questService.ShowQuestStatus(party, _fullQuest.Quest.IdKey, false, true, false);
            bool iconVisible = map != null && await _questService.QuestIsActive(party, _fullQuest.Quest.IdKey);

            _clientEntityService.SetActive(IsActiveImage, iconVisible);
            _uiService.SetText(Text, await _questService.ShowQuestStatus(party, _fullQuest.Quest.IdKey, false, true, false));
            _isActiveQuest = iconVisible;
            if (_fullQuest.IsComplete())
            {
                _isActiveQuest = false;
            }
        }
    }

}


