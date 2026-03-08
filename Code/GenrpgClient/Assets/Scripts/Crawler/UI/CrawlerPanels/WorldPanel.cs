using Genrpg.Shared.Client.Interfaces;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{

    public class ShowWorldPanelImage : IClientEvent
    {
        public string SpriteName;
    }

    public class WorldPanel : BaseBehaviour
    {

        public override void Init()
        {
        }


        public void ApplyEffect(string effectName, float duration)
        {
        }
    }
}


