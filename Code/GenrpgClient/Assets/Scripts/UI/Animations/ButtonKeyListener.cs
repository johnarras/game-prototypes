using Assets.Scripts.Awaitables;
using Assets.Scripts.UI.ClientEvents;
using System;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.UI.Animations
{
    [RequireComponent(typeof(GButton))]
    public class ButtonKeyListener : BaseBehaviour
    {
        private IAwaitableService _awaitableService = null;

        public GButton Button;
        public char Key;

        private Action _action;

        public override void Init()
        {
            base.Init();

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
            if (char.ToLower(ck.Key) != char.ToLower(Key))
            {
                return;
            }
            _awaitableService.ForgetAwaitable(OnClickKeyAsync(GetToken()));
        }

        private async Awaitable OnClickKeyAsync(CancellationToken token)
        {

            if (_action != null)
            {
                _action();
            }
            Button.image.color = Button.colors.pressedColor;
            await Awaitable.NextFrameAsync(token);
            Button.image.color = Button.colors.normalColor;
        }
    }
}
