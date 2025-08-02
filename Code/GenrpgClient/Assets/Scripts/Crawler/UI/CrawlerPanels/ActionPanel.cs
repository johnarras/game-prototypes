using Assets.Scripts.Crawler.UI.ActionUI;
using Assets.Scripts.UI.Abstractions;
using Assets.Scripts.UI.Core;
using Assets.Scripts.UI.Crawler.ActionUI;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{
    public class TextAction
    {
        public string Text { get; set; }
        public Action ClickAction { get; set; }
    }
    public class ActionPanel : BaseBehaviour
    {

        public GameObject Content;
        public GScrollRect ScrollRect;
        public GameObject Parent;
        public const int InputCount = 3;

        public ActionPanelRow PanelRow;
        public ActionPanelGrid PanelGrid;
        public ActionPanelRow PanelButton;

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

            _clientEntityService.DestroyAllChildren(Content);

            _clientEntityService.SetActive(Parent, !stateData.HideBigPanels);

            if (stateData.HideBigPanels)
            {
                return;
            }

            List<CrawlerStateAction> buttonActions = new List<CrawlerStateAction>();

            for (int a = 0; a < stateData.Actions.Count; a++)
            {

                CrawlerStateAction action = stateData.Actions[a];

                if (action.HideText || (action.Key == CharCodes.Escape && stateData.HasInput()))
                {
                    continue;
                }

                if (!action.ForceButton || action.ForceText || (!action.ForceButton && !action.RowFiller && (action.Key == CharCodes.Escape || action.Key == CharCodes.Space ||
                    string.IsNullOrEmpty(action.Text) || action.Text.Length >= 20 ||
                    action.NextState == ECrawlerStates.None)))
                {
                    ActionPanelRow actionPanelRow = _clientEntityService.FullInstantiate(PanelRow);
                    _clientEntityService.AddToParent(actionPanelRow, Content);
                    actionPanelRow.SetAction(new CrawlerStateWithAction() { State = stateData, Action = stateData.Actions[a] });
                    _subObjects.Add(actionPanelRow);    
                }
                else
                {
                    buttonActions.Add(action);
                }
            }

            ActionPanelGrid grid = null;

            for (int a = 0; a < buttonActions.Count; a++)
            {
                if (grid == null)
                {
                    grid = _clientEntityService.FullInstantiate(PanelGrid);
                    _clientEntityService.AddToParent(grid, Content);
                    grid.SetData(stateData.UseSmallerButtons);  
                    _subObjects.Add(grid);
                }

                CrawlerStateAction action = buttonActions[a];

                if (action.RowFiller)
                {
                    grid = null;
                    continue;
                }

                CrawlerStateWithAction stateAction = new CrawlerStateWithAction()
                {
                    State = stateData,
                    Action = action,
                };

                ActionPanelRow button = _clientEntityService.FullInstantiate(PanelButton);
                _clientEntityService.AddToParent(button, grid.GetContentRoot());
                button.SetAction(stateAction);
                _subObjects.Add(button);
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

        public void Clear()
        {
            _clientEntityService.DestroyAllChildren(Content);
            _subObjects.Clear();
        }
    }
}
