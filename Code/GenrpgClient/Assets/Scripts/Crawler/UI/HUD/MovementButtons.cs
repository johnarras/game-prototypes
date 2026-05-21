using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Crawler.Shared.GameEvents;
using Assets.Scripts.UI.Animations;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Services;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Crawler.UI.HUD
{
    public class MovementButtons : BaseBehaviour
    {
        private ICrawlerMoveService _moveService = null;
        private ICrawlerService _crawlerService = null;

        public GButton TurnLeftButton;
        public GButton ForwardButton;
        public GButton TurnRightButton;
        public GButton StrafeLeftButton;
        public GButton BackButton;
        public GButton StrafeRightButton;

        public override void Init()
        {
            base.Init();

            _dispatcher.AddListener<SetupMovementButtons>(OnSetupMovementButtons, GetToken());
            SetupMovementKeys(false);
        }


        private void OnSetupMovementButtons(SetupMovementButtons setup)
        {
            SetupMovementKeys(true);
        }

        public void SetupMovementKeys(bool setupCodesNow)
        {


            IReadOnlyList<MovementKeyCode> keys = _moveService.GetMovementKeyCodes(setupCodesNow);
            SetupMovementKey(TurnLeftButton, MovementKeyNames.TurnLeft, keys);
            SetupMovementKey(ForwardButton, MovementKeyNames.Forward, keys);
            SetupMovementKey(TurnRightButton, MovementKeyNames.TurnRight, keys);
            SetupMovementKey(StrafeLeftButton, MovementKeyNames.StrafeLeft, keys);
            SetupMovementKey(BackButton, MovementKeyNames.Backward, keys);
            SetupMovementKey(StrafeRightButton, MovementKeyNames.StrafeRight, keys);
        }

        private void SetupMovementKey(GButton button, string codeName, IReadOnlyList<MovementKeyCode> keys)
        {
            if (button == null)
            {
                return;
            }

            MovementKeyCode kc = keys.FirstOrDefault(x => x.Name == codeName);

            if (kc != null)
            {
                _uiService.SetButton(button, name, () =>
                {
                    if (_crawlerService.GetState() == ECrawlerStates.ExploreWorld)
                    {
                        _moveService.AddMovementKeyInput(kc.Key, GetToken());
                    }
                });

                ButtonKeyListener listener = _clientEntityService.GetComponent<ButtonKeyListener>(button);

                if (listener != null)
                {
                    listener.SetKey(kc.Key);
                }
            }
        }
    }
}


