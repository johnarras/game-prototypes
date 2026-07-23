using OxDb.Client.Assets.Scripts.UI.Help.ClientEvents;
using UnityEngine;

namespace OxDb.Client.Assets.Scripts.UI.Help
{
    public class HelpPanel : BaseBehaviour
    {

        public GameObject Content;

        public GText Info;

        public GButton CloseButton;

        public override void Init()
        {

            _dispatcher.AddListener<ShowHelpPanels>(OnShowHelpPanels, GetToken());
            _dispatcher.AddListener<HideHelpPanels>(OnHideHelpPanels, GetToken());
            _uiService.SetButton(CloseButton, GetName(), OnClickPanel);
            SetVisible(false);
        }

        private void OnShowHelpPanels(ShowHelpPanels show)
        {
            SetVisible(true);
        }

     
        private void OnHideHelpPanels(HideHelpPanels hide)
        {
            SetVisible(false);
        }

        private void OnClickPanel()
        {
            _dispatcher.Dispatch(new HideHelpPanels());
        }

        private void SetVisible(bool visible)
        {
            _clientEntityService.SetActive(Content, visible);
        }
    }
}
