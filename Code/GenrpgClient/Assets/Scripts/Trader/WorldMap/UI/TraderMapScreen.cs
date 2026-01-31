using Assets.Scripts.Trader.UI.TraderMapUI;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Travel.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.UI.TradeMapUI
{
    public class TraderMapScreen : BaseScreen
    {
        public TraderMapCityButton ButtonPrefab;

        public GImage MapImage;

        public GameObject CityAnchor;

        private List<TraderMapCityButton> _buttons = new List<TraderMapCityButton>();


        private List<long> _cityIdsClicked = new List<long>();

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {


            await LoadCityImages();
        }

        private async Awaitable LoadCityImages()
        {
            IReadOnlyList<City> cities = _gameData.Get<CitySettings>(_gs.ch).GetData();

            int texWidth = MapImage.mainTexture.width;
            int texHeight = MapImage.mainTexture.height;

            float spriteWidth = MapImage.preferredWidth;
            float spriteHeight = MapImage.preferredHeight;


            float xscale = 1;
            float yscale = 1;

            float xdelta = 0;
            float ydelta = 0;

            foreach (City city in cities)
            {
                int xpix = (int)(xdelta + city.MapPixelX * xscale);
                int ypix = -(int)(ydelta + city.MapPixelY * yscale);

                TraderMapCityButton button = _clientEntityService.FullInstantiate<TraderMapCityButton>(ButtonPrefab);

                button.InitCity(this, city);

                _clientEntityService.AddToParent(button, CityAnchor);

                _buttons.Add(button);

                button.transform.localPosition = new Vector3(xpix, ypix, 0);
            }

            await Task.CompletedTask;
        }

        public void ClickCityUI(long cityId)
        {

            if (_cityIdsClicked.Contains(cityId))
            {
                return;
            }
            _cityIdsClicked.Add(cityId);
            while (_cityIdsClicked.Count > 2)
            {
                _cityIdsClicked.RemoveAt(0);
            }


            if (_cityIdsClicked.Count != 2)
            {
                return;
            }
        }
    }
}


