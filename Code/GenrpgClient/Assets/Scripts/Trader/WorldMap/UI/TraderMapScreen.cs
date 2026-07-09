using Assets.Scripts.Trader.UI.TraderMapUI;
using OxDb.SharedGame.Trader.Cities.Settings;
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
            float zscale = 1;

            float xdelta = 0;
            float zdelta = 0;

            foreach (City city in cities)
            {
                int xpix = (int)(xdelta + city.MapPixelX * xscale);
                int zpix = -(int)(zdelta + city.MapPixelZ * zscale);

                TraderMapCityButton button = _clientEntityService.FullInstantiate<TraderMapCityButton>(ButtonPrefab);

                button.InitCity(this, city);

                _clientEntityService.AddToParent(button, CityAnchor);

                _buttons.Add(button);

                button.transform.localPosition = new Vector3(xpix, zpix, 0);
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


