using Assets.Scripts.Assets.ObjectPools;
using Assets.Scripts.ClientEvents;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Info.Services;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Info.UI
{
    public class InfoPanel : BaseBehaviour
    {
        private IInfoService _infoService = null;
        private IObjectPool _objectPool = null;

        public GameObject Parent;
        public GameObject InfoAnchor;
        public bool IsTooltipPanel;

        private Stack<List<string>> _infoStack = new Stack<List<string>>();
        private List<string> _currentInfo = null;


        private List<InfoPanelRow> _rows = new List<InfoPanelRow>();

        public string Subdirectory = "CrawlerInfo";
        public string RowPrefabName = "InfoPanelRow";

        public override void Init()
        {
            if (IsTooltipPanel)
            {
                _clientEntityService.SetActive(Parent, false);
                _dispatcher.AddListener<ShowInfoPanelEvent>(OnShowTooltip, GetToken());
                _dispatcher.AddListener<HideInfoPanelEvent>(OnHideTooltip, GetToken());
            }

        }


        public void ClearInfo()
        {
            foreach (InfoPanelRow row in _rows)
            {
                _objectPool.ReturnObject(row);
            }
            _rows.Clear();
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

                _objectPool.CheckoutObject(InfoAnchor, AssetCategoryNames.UI, RowPrefabName, OnLoadRow, line, GetToken(), Subdirectory);

            }
        }

        private void OnLoadRow(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            string txt = data as string;

            if (go == null)
            {
                return;
            }

            InfoPanelRow row = go.GetComponent<InfoPanelRow>();

            if (row == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            row.InitData(this, txt);
            _rows.Add(row);
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


