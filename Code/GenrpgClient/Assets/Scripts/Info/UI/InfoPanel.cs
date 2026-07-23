using OxDb.Client.Assets.Constants;
using OxDb.Client.Assets.ObjectPools;
using OxDb.Client.ClientEvents;
using OxDb.Client.Entities.UI;
using OxDb.SharedGame.Crawler.Info.Services;
using OxDb.SharedGame.Input.PlayerData;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace OxDb.Client.Info.UI
{

    public enum EInfoPanelDisplayReason
    {
        Pointer = 0,
        Click = 1,
    }


    public class InfoPanel : BaseBehaviour
    {
        private IInfoService _infoService = null;
        private IObjectPool _objectPool = null;
        protected IInputService _inputService = null;

        public GButton BackButton;

        public GameObject Parent;
        public GameObject InfoAnchor;
        public bool IsTooltipPanel;


        public EntityIcon Icon;

        private Stack<ShowInfoPanelArgs> _infoStack = new Stack<ShowInfoPanelArgs>();
        private ShowInfoPanelArgs _currentArgs = null;


        private List<InfoPanelRow> _rows = new List<InfoPanelRow>();

        public string Subdirectory = "CrawlerInfo";
        public string RowPrefabName = "InfoPanelRow";

        public override void Init()
        {
            if (IsTooltipPanel)
            {
                _clientEntityService.SetActive(Parent, false);
                _dispatcher.AddListener<ShowInfoPanelArgs>(OnShowTooltip, base.GetToken());
                _dispatcher.AddListener<HideInfoPanelEvent>(OnHideTooltip, GetToken());
            }
            _uiService.SetButton(BackButton, GetName(), PopInfoStack);
        }

        public void ClearInfo()
        {
            foreach (InfoPanelRow row in _rows)
            {
                _objectPool.ReturnObject(row);
            }
            _rows.Clear();

            Icon.SetEntityData(0, 0, 0);

        }

        public void ShowLines(ShowInfoPanelArgs args, EInfoPanelDisplayReason reason)
        {
            if (args.Lines.Count < 1)
            {
                return;
            }

            if (_currentArgs != null && _currentArgs.Lines.Count > 0 &&
                reason == EInfoPanelDisplayReason.Pointer && _inputService.ModifierIsActive(KeyComm.ShiftName))
            {
                return;
            }


            ClearInfo();

            _clientEntityService.SetActive(Parent, true);
            if (_currentArgs != null && _currentArgs.Lines.Count > 0)
            {
                _infoStack.Push(_currentArgs);
            }
            _currentArgs = args;

            foreach (string line in args.Lines)
            {
                _objectPool.CheckoutObject(InfoAnchor, AssetCategoryNames.UI, RowPrefabName, OnLoadRow, line, GetToken(), Subdirectory);
            }

            Icon.SetEntityData(args.EntityTypeId, args.EntityId, 0);
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

            row.SetData(this, txt);
            _rows.Add(row);
        }

        public void PopInfoStack()
        {
            if (_infoStack.TryPop(out ShowInfoPanelArgs currArgs))
            {
                _currentArgs = null;
                ShowLines(currArgs, EInfoPanelDisplayReason.Click);
            }
            else
            {
                HideTooltipInternal();
            }

        }

        public void ShowOverview(string entityTypeName)
        {

        }

        public void ShowEntityInfo(long entityTypeId, long entityId, EInfoPanelDisplayReason reason)
        {
            ShowInfoPanelArgs args = _infoService.GetInfoPanelArgs(entityTypeId, entityId);
            ShowLines(args, reason);
        }

        public void ClearStack()
        {
            _infoStack.Clear();
            ClearInfo();
        }
        private void OnShowTooltip(ShowInfoPanelArgs args)
        {
            if (args.Lines.Count > 0)
            {
                ShowLines(args, args.Reason);
            }
            else if (args.EntityTypeId > 0 && args.EntityId > 0)
            {
                ShowEntityInfo(args.EntityTypeId, args.EntityId, args.Reason);
            }
            else
            {
                return;
            }
            _clientEntityService.SetActive(Parent, true);
        }

        private void OnHideTooltip(HideInfoPanelEvent hideEvent)
        {
            HideTooltipInternal();
        }

        private void HideTooltipInternal()
        {
            if (!_inputService.ModifierIsActive(KeyComm.ShiftName))
            {
                ClearInfo();
                ClearStack();
                _clientEntityService.SetActive(Parent, false);
            }
        }
    }
}


