

using Assets.Scripts.Crawler.UI.WorldUI;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.UI.Interfaces;
using Assets.Scripts.Crawler.Services.CrawlerMaps;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{

    public class ShowWorldPanelImage
    {
        public string SpriteName;
    }

    public class WorldPanel : BaseBehaviour
    {
        private ICrawlerMapService _crawlerMapService;
        private ICrawlerWorldService _worldService;


        private IImage _peacefulImage;
        private IImage _noMagicImage;

        private IButton _closeTooltipButton;

        private WorldPanelCompass _panelCompass;



        public override void Init()
        {
        }


        public void ApplyEffect(string effectName, float duration)
        {
        }
    }
}
