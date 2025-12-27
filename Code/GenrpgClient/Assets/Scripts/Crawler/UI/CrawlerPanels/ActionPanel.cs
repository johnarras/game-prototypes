using Assets.Scripts.Assets.ObjectPools;
using Assets.Scripts.Awaitables;
using Assets.Scripts.Crawler.UI.ActionUI;
using Assets.Scripts.UI.Abstractions;
using Assets.Scripts.UI.Core;
using Assets.Scripts.UI.Crawler.ActionUI;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{
    public class TextAction
    {
        public string Text { get; set; }
        public Action ClickAction { get; set; }
    }
    public class ActionPanel : BaseBehaviour
    {

        protected IAwaitableService _awaitableService = null;
        protected IObjectPool _objectPool = null;


        public GameObject Content;
        public GScrollRect ScrollRect;
        public GameObject Parent;
        public const int InputCount = 3;

        public ActionPanelRow PanelRow;
        public ActionPanelGrid PanelGrid;
        public ActionPanelRow PanelButton;

        public string RowPrefabName = "ActionPanelRow";
        public string ButtonPrefabName = "ActionPanelButton";

        private const string Subdirectory = "CrawlerAction";

        public List<LabeledInputField> InputFields = new List<LabeledInputField>();

        private CrawlerStateData _nextStateData = null;

        private List<object> _subObjects = new List<object>();

        public override void Init()
        {
            _dispatcher.AddListener<CrawlerStateData>(OnNewStateData, GetToken());
        }


        private void OnNewStateData(CrawlerStateData stateData)
        {
            _nextStateData = stateData;

            Clear();

            _clientEntityService.SetActive(Parent, !stateData.HideBigPanels);

            if (_nextStateData.HideBigPanels)
            {
                return;
            }

            List<CrawlerStateAction> buttonActions = new List<CrawlerStateAction>();

            for (int a = 0; a < _nextStateData.Actions.Count; a++)
            {

                CrawlerStateAction action = _nextStateData.Actions[a];

                if (action.HideText || (action.Key == Key.Escape && stateData.HasInput()))
                {
                    continue;
                }

                if (!action.ForceButton || action.ForceText || (!action.ForceButton && !action.RowFiller && (action.Key == Key.Escape || action.Key == Key.Space ||
                    string.IsNullOrEmpty(action.Text) || action.Text.Length >= 20 ||
                    action.NextState == ECrawlerStates.None)))
                {
                    CrawlerStateWithAction csa = new CrawlerStateWithAction() { State = stateData, Action = stateData.Actions[a] };

                    _objectPool.CheckoutObject(Content, AssetCategoryNames.UI, RowPrefabName, OnLoadRow, csa, GetToken(), Subdirectory);

                }
                else
                {
                    buttonActions.Add(action);
                }
            }
            ActionPanelGrid grid = null;

            for (int a = 0; a < buttonActions.Count; a++)
            {
                CrawlerStateAction action = buttonActions[a];

                if (action.RowFiller)
                {
                    grid = null;
                    continue;
                }

                if (grid == null)
                {
                    grid = _clientEntityService.FullInstantiate(PanelGrid);
                    _clientEntityService.AddToParent(grid, Content);
                    grid.SetData(stateData.UseSmallerButtons);
                    _subObjects.Add(grid);
                }

                CrawlerStateWithAction stateAction = new CrawlerStateWithAction()
                {
                    State = stateData,
                    Action = action,
                };

                _objectPool.CheckoutObject(grid.GetContentRoot(), AssetCategoryNames.UI, ButtonPrefabName, OnLoadRow, stateAction, GetToken(), Subdirectory);

            }

            List<CrawlerInputData> stateInputs = stateData.Inputs;

            for (int i = 0; i < InputFields.Count; i++)
            {
                InputFields[i].SetLabel("");
                InputFields[i].SetPlaceholder("");
                InputFields[i].SetInputText("");
                _clientEntityService.SetActive(InputFields[i], false);
            }

            for (int i = 0; i < InputFields.Count && i < stateInputs.Count; i++)
            {
                _clientEntityService.SetActive(InputFields[i], true);
                stateInputs[i].InputField = InputFields[i];
                InputFields[i].SetLabel(stateInputs[i].InputLabel);
                InputFields[i].SetPlaceholder(stateData.InputPlaceholderText);
                InputFields[i].SetInputText("");
            }

            _uiService.ScrollToBottom(ScrollRect);
        }

        private void OnLoadButton(object obj, object data, CancellationToken token)
        {
            OnLoadRow(obj, data, token);
        }

        private void OnLoadRow(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;
            if (go == null)
            {
                return;
            }

            ActionPanelRow apr = go.GetComponent<ActionPanelRow>();

            CrawlerStateWithAction csa = data as CrawlerStateWithAction;

            if (apr == null || csa == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            apr.SetAction(csa);
            _subObjects.Add(apr);
        }

        public void Clear()
        {

            foreach (object obj in _subObjects)
            {
                _objectPool.ReturnObject(obj);
            }


            _clientEntityService.DestroyAllChildren(Content);
            _subObjects.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}


