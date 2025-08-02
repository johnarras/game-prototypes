using Assets.Scripts.ClientEvents;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Info.UI;
using Genrpg.Shared.Crawler.Info.Services;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Info.UI
{
    public class InfoPanel : BaseBehaviour
    {
        private IInfoService _infoService;

        public GameObject Parent;
        public GameObject InfoAnchor;
        public InfoPanelRow InfoText;
        public bool IsTooltipPanel;

        private Stack<List<string>> _infoStack = new Stack<List<string>>();
        private List<string> _currentInfo = null;

        public override void Init()
        {
            _clientEntityService.SetActive(InfoText?.gameObject, false);
            if (IsTooltipPanel)
            {
                _clientEntityService.SetActive(Parent, false);
                _dispatcher.AddListener<ShowInfoPanelEvent>(OnShowTooltip, GetToken());
                _dispatcher.AddListener<HideInfoPanelEvent>(OnHideTooltip, GetToken());
            }
           
        }


        public void ClearInfo()
        {
            _clientEntityService.DestroyAllChildren(InfoAnchor);
        }

        public void ShowLines(List<string> lines)
        {
            if (lines.Count < 1)
            {
                return;
            }

            ClearInfo();

            _clientEntityService.SetActive(Parent, true);
            if (_currentInfo != null && _currentInfo.Count > 0)
            {
                _infoStack.Push(_currentInfo);
            }
            _currentInfo = lines;

            foreach (string line in lines)
            {
                InfoPanelRow listItem = _clientEntityService.FullInstantiate<InfoPanelRow>(InfoText);

                _clientEntityService.AddToParent(listItem, InfoAnchor);

                listItem.InitData(this, line);
            }
        }

        public void PopInfoStack()
        {
            if (_infoStack.TryPop(out List<string> currList))
            {
                _currentInfo = null;
                ShowLines(currList);
            }
            else
            {
                ClearInfo();
            }

        }

        public void ShowOverview(string entityTypeName)
        {

        }

        public void ShowInfo(long entityTypeId, long entityId)
        {
            List<string> lines = _infoService.GetInfoLines(entityTypeId, entityId);
            ShowLines(lines);
        }

        public void ClearStack()
        {
            _infoStack.Clear();
        }
        private void OnShowTooltip(ShowInfoPanelEvent showEvent)
        {
            if (showEvent.EntityTypeId > 0 && showEvent.EntityId > 0)
            {
                ShowInfo(showEvent.EntityTypeId, showEvent.EntityId);
            }
            else if (showEvent.Lines.Count > 0)
            {
                ShowLines(showEvent.Lines);
            }
            else
            {
                return;
            }
            _clientEntityService.SetActive(Parent, true);
        }
        
        private void OnHideTooltip(HideInfoPanelEvent hideEvent)
        {
            _clientEntityService.SetActive(Parent, false);
        }
    }
}
