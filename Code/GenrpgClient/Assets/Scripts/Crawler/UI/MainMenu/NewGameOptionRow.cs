using Genrpg.Shared.Crawler.Options.Settings;

namespace Assets.Scripts.Crawler.UI.MainMenu
{
    public class NewGameOptionRow : BaseBehaviour
    {

        public GToggle Toggle;

        private CrawlerOption _option = null;

        public void Init(CrawlerOption option)
        {
            _option = option;
            Toggle.Init(option.Name + ": " + option.Desc, option.DefaultForNewGame);
        }

        public long GetOptionId()
        {
            return _option?.IdKey ?? 0;
        }

        public bool IsOptionSet()
        {
            return Toggle.IsOn();
        }

    }
}


