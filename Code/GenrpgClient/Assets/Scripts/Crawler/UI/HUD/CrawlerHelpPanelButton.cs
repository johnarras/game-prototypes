using OxDb.Client.Assets.Scripts.UI.Help.ClientEvents;
using System;
using System.Collections.Generic;
using System.Text;

namespace OxDb.Client.Assets.Scripts.Crawler.UI.HUD
{
    public class CrawlerHelpPanelButton : BaseBehaviour
    {
        public GButton Button;

        private bool _showingHelpPanels = false;
        public override void Init()
        {
            _uiService.SetButton(Button, GetName(), OnClickButton);
            _dispatcher.AddListener<ShowHelpPanels>(OnShowHelpPanels, GetToken());
            _dispatcher.AddListener<HideHelpPanels>(OnHideHelpPanels, GetToken());
        }

        private void OnShowHelpPanels(ShowHelpPanels show)
        {
            _showingHelpPanels = true;
        }


        private void OnHideHelpPanels(HideHelpPanels hide)
        {
            _showingHelpPanels = false;
        }

        private void OnClickButton()
        {
            if (_showingHelpPanels)
            {
                _dispatcher.Dispatch(new HideHelpPanels());
            }
            else
            {
                _dispatcher.Dispatch(new ShowHelpPanels());
            }    
        }
    }
}
