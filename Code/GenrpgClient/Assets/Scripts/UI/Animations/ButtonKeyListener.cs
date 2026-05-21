using Assets.Scripts.Awaitables;
using Assets.Scripts.UI.ClientEvents;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.UI.Animations
{
    [RequireComponent(typeof(GButton))]
    public class ButtonKeyListener : BaseBehaviour
    {
        private IAwaitableService _awaitableService = null;

        public List<GImage> Images = new List<GImage>();
        public GButton Button;
        public Key Key;

        private Action _action;
        public bool SuppressAction = false;
        public override void Init()
        {
            base.Init();

            SetKey(Key);
        }

        public void SetKey(Key key)
        {
            Key = key;
            if (Key > 0 && Button != null)
            {
                _dispatcher.AddListener<ClickKey>(OnClickKey, GetToken());
            }
        }

        public void SetClickAction(Action action)
        {
            _action = action;
        }

        private void OnClickKey(ClickKey ck)
        {
            if (ck.Key != Key)
            {
                return;
            }
            _awaitableService.ForgetAwaitable(OnClickKeyAsync(GetToken()));
        }

        private async Awaitable OnClickKeyAsync(CancellationToken token)
        {

            if (_action != null && !SuppressAction)
            {
                _action();
            }

            foreach (GImage image in Images)
            {
                image.color = Button.colors.pressedColor;
            }

            await Awaitable.WaitForSecondsAsync(0.1f);
            foreach (GImage image in Images)
            {
                image.color = Button.colors.normalColor;
            }
        }
    }
}


