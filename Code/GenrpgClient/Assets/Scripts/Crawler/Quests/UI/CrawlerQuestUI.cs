using Assets.Scripts.Awaitables;
using Assets.Scripts.Crawler.Quests.ClientEvents;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Quests.Entities;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Worlds.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Quests.UI
{
    public class CrawlerQuestUI : BaseBehaviour
    {
        private ICrawlerService _crawlerService = null;
        private IAwaitableService _awaitableService = null;
        private ICrawlerWorldService _worldService = null;

        public GameObject Anchor;

        public CrawlerQuestRow RowPrefab;

        private List<CrawlerQuestRow> _currentRows = new List<CrawlerQuestRow>();

        public override void Init()
        {
            base.Init();

            ShowQuestRows();

            _dispatcher.AddListener<UpdateQuestUI>(OnUpdateQuestUI, GetToken());
        }

        private void OnUpdateQuestUI(UpdateQuestUI updateQuestUI)
        {
            ShowQuestRows();
        }

        private void ShowQuestRows()
        {
            _awaitableService.ForgetAwaitable(ShowQuestRowsAsync(GetToken()));
        }

        private async Awaitable ShowQuestRowsAsync(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
            PartyData party = _crawlerService.GetParty();
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            List<long> validQuestIds = new List<long>();
            foreach (PartyQuest partyQuest in party.Quests)
            {
                CrawlerQuest quest = world.GetQuest(partyQuest.CrawlerQuestId);

                if (quest == null)
                {
                    party.Quests.Remove(partyQuest);
                    continue;
                }

                validQuestIds.Add(quest.IdKey);

                CrawlerQuestRow row = _currentRows.FirstOrDefault(x => x.GetQuestId() == quest.IdKey);

                if (row != null)
                {
                    row.UpdateData();
                }
                else
                {
                    FullQuest fullQuest = new FullQuest()
                    {
                        Quest = quest,
                        Progress = partyQuest,
                        ReturnState = ECrawlerStates.ExploreWorld,
                    };

                    row = _clientEntityService.FullInstantiate<CrawlerQuestRow>(RowPrefab);
                    _clientEntityService.AddToParent(row, Anchor);
                    _currentRows.Add(row);
                    row.SetData(fullQuest);
                }
            }

            List<CrawlerQuestRow> removeRows = new List<CrawlerQuestRow>();

            foreach (CrawlerQuestRow row in _currentRows)
            {
                if (!validQuestIds.Contains(row.GetQuestId()))
                {
                    removeRows.Add(row);
                }
            }
            foreach (CrawlerQuestRow row in removeRows)
            {
                _clientEntityService.Destroy(row.gameObject);
                _currentRows.Remove(row);
            }
        }
    }
}
