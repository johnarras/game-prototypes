using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using System;
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

            SetupMovementKey(TurnLeftButton);
            SetupMovementKey(ForwardButton);
            SetupMovementKey(TurnRightButton);
            SetupMovementKey(StrafeLeftButton);
            SetupMovementKey(BackButton);
            SetupMovementKey(StrafeRightButton);
        }

        private void SetupMovementKey(GButton button)
        {
            if (button == null)
            {
                return;
            }

            IReadOnlyList<MovementKeyCode> keys = _moveService.GetMovementKeyCodes();

            string codeName = button.name.Replace("Button", "");

            MovementKeyCode kc = keys.FirstOrDefault(x=>x.Name == codeName);    

            if (kc != null)
            {
                _uiService.SetButton(button, name, () => 
                {
                    if (_crawlerService.GetState() == ECrawlerStates.ExploreWorld)
                    {
                        _moveService.AddMovementKeyInput(kc.Key, GetToken());
                    }
                });
            }
        }
    }
}
