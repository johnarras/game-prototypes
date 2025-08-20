
using Assets.Scripts.Awaitables;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Quests.Entities;
using Genrpg.Shared.Crawler.Quests.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Quests.UI
{
    public class CrawlerQuestRow : BaseBehaviour
    {

        private ICrawlerService _crawlerService = null;
        private ICrawlerQuestService _questService = null;
        private IAwaitableService _awaitableService = null;
        private ICrawlerWorldService _worldService = null;

        public GImage IsActiveImage;
        public GText Text;
        public GButton Button;

        private FullQuest _fullQuest = null;
        private bool _isActiveQuest = false;

        public void SetData(FullQuest fullQuest)
        {
            _fullQuest = fullQuest;
            _uiService.SetButton(Button, GetType().Name,
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
            _awaitableService.ForgetAwaitable(ShowDataAsync(GetToken()));
        }

        public bool IsActiveQuest()
        {
            return _isActiveQuest;
        }

        private async Awaitable ShowDataAsync(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();

            PartyData party = _crawlerService.GetParty();
            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            string questStatus = await _questService.ShowQuestStatus(party, _fullQuest.Quest.IdKey, false, true, false);
            bool iconVisible = true;
            if (map == null || map.BaseCrawlerMapId != _fullQuest.Quest.CrawlerMapId)
            {
                iconVisible = false;
            }
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
