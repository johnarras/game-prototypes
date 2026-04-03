using Assets.Scripts.UI.ClientEvents;
using UnityEngine;

namespace Assets.Scripts.UI.ClientEventListeners
{
    public class BackScreenToggle : BaseBehaviour
    {
        public GameObject ToggleTarget;


        public override void Init()
        {
            _dispatcher.AddListener<ShowBackScreen>(OnShowBackScreen, GetToken());
            _dispatcher.AddListener<HideBackScreen>(OnHideBackScreen, GetToken());
        }

        private void OnShowBackScreen(ShowBackScreen showBack)
        {
            _clientEntityService.SetActive(ToggleTarget, false);
        }

        private void OnHideBackScreen(HideBackScreen hideBack)
        {
            _clientEntityService.SetActive(ToggleTarget, true);
        }
    }
}
