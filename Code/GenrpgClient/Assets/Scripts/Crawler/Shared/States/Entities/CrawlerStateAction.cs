using Genrpg.Shared.Crawler.States.Constants;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Genrpg.Shared.Crawler.States.Entities
{
    public class CrawlerStateAction
    {
        public CrawlerStateAction(string text,
            Key key = Key.None,
            ECrawlerStates nextState = ECrawlerStates.None,
            Action onClickAction = null,
            object extraData = null,
            string spriteName = null,
            Action<GameObject> pointerEnterAction = null,
            Action<GameObject> pointerExitAction = null,
            bool rowFiller = false,
            bool forceButton = false,
            bool forceText = false,
            bool hideText = false)
        {
            Text = text;
            Key = key;
            NextState = nextState;
            OnClickAction = onClickAction;
            SpriteName = spriteName;
            ExtraData = extraData;
            OnPointerEnter = pointerEnterAction;
            OnPointerExit = pointerExitAction;
            RowFiller = rowFiller;
            ForceButton = forceButton;
            ForceText = forceText;
            HideText = hideText;
        }

        public string Text { get; private set; }
        public Key Key { get; private set; }
        public ECrawlerStates NextState { get; private set; }
        public Action OnClickAction { get; private set; }
        public string SpriteName { get; private set; }
        public object ExtraData { get; private set; }
        public Action<GameObject> OnPointerEnter { get; private set; }
        public Action<GameObject> OnPointerExit { get; private set; }
        public bool RowFiller { get; private set; }
        public bool ForceButton { get; private set; }
        public bool ForceText { get; private set; }
        public bool HideText { get; private set; }

    }
}


