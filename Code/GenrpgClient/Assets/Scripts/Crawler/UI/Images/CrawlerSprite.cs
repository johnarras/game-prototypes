using Assets.Scripts.Assets.Textures;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.Images
{
    public class CrawlerSprite : BaseBehaviour
    {
        private ICrawlerMapService _crawlerMapService;


        public bool IsBG;
        public AnimatedSprite Sprite;

        public GImage FrontBacking;

        public override void Init ()
        {
            _dispatcher.AddListener<CrawlerStateData>(OnNewStateData, GetToken());

            AddListener<ShowWorldPanelImage>(OnShowWorldPanelImage);
        }

        private void OnNewStateData(CrawlerStateData stateData)
        {
            ShowImage(stateData.WorldSpriteName, stateData.BGSpriteName, stateData.ClearBGImage);
        }

        private void ShowImage(string worldImageName, string bgImageName, bool clearBGImage)
        {
            if (IsBG)
            {
                if (!string.IsNullOrEmpty(bgImageName))
                {
                    Sprite.SetImage(bgImageName);
                }
                else if (clearBGImage)
                {
                    Sprite.SetImage(null);
                }
            }
            else
            {
                Sprite.SetImage(worldImageName);
                _clientEntityService.SetActive(FrontBacking, !string.IsNullOrEmpty(worldImageName));
            }
        }

        private void OnShowWorldPanelImage(ShowWorldPanelImage imageToShow)
        {
            if (IsBG)
            {
                return;
            }

            ShowImage(imageToShow.SpriteName, null, false);
        }      
    }
}
