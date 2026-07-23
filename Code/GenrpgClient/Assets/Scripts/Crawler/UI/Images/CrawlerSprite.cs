using OxDb.Client.Assets.Textures;
using OxDb.Client.UI.Crawler.CrawlerPanels;

namespace OxDb.Client.Crawler.UI.Images
{
    public class CrawlerSprite : BaseBehaviour
    {
        public bool IsBG;
        public AnimatedSprite Sprite;

        public GImage FrontBacking;

        public override void Init()
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


