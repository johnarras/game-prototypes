using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using Assets.Scripts.UI.Crawler.StatusUI;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{
    public class StatusPanel : BaseBehaviour
    {
        private ICrawlerService _crawlerService;

        public List<PartyMemberStatusRow> Rows = new List<PartyMemberStatusRow>();

        public GameObject Content;

        public override void Init()
        {
            _dispatcher.AddListener<CrawlerStateData>(OnNewStateData, GetToken());

            for (int i = 0; i < Rows.Count; i++)
            {
                Rows[i].SetData(i + 1);
            }


            UpdatePartyData();
        }

        private void OnRefreshPartyStatus(RefreshPartyStatus status)
        {
            UpdatePartyData();
        }
        private void OnRefreshUnitStatus(RefreshUnitStatus status)
        {
            RefreshUnit(status.Unit, status.ElementTypeId);
        }

        private void UpdatePartyData(int partyIndexToUpdate = 0, long elementTypeId = 0)
        { 
            PartyData party = _crawlerService.GetParty();

            for (int r = 0; r < Rows.Count; r++)
            {
                if (partyIndexToUpdate > 0 && r+1 != partyIndexToUpdate)
                {
                    continue;
                }
                Rows[r].UpdateData(elementTypeId);
            }
        }

        private void OnNewStateData(CrawlerStateData stateData)
        {
            UpdatePartyData();
        }

        public void RefreshUnit(CrawlerUnit unit, long elementTypeId = 0)
        {
            if (unit is PartyMember member)
            {
                UpdatePartyData(member.PartySlot, elementTypeId);
            }
        }
    }
}
