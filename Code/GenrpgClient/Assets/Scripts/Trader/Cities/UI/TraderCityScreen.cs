using OxDb.Client.Assets.Constants;
using OxDb.Client.Trader.Cities.UI;
using OxDb.Client.UI.ScreenSystem;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Cities.Settings;
using OxDb.SharedGame.Trader.Cultures.Constants;
using OxDb.SharedGame.Trader.Cultures.Settings;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Trader.UI.Cities
{
    public class TraderCityScreen : TypedArgScreen<TraderCityScreenArgs>
    {

        public GameObject PanelAnchor;
        private ICaravanService _caravanService = null;

        public GText ScreenHeader;
        private City _city = null;
        private TraderCityPanel _panel = null;
        protected override async Task OnStartOpen(TraderCityScreenArgs args, CancellationToken token)
        {
            if (args != null)
            {
                _city = _gameData.Get<CitySettings>(_gs.ch).Get(args.CityId);
            }

            if (_city == null)
            {

                CaravanPosition pos = await _caravanService.GetPosition(_gs.ch);

                _city = pos.GetCurrentCity();
            }

            if (_city == null)
            {
                StartClose();
                return;
            }

            CultureType culture = _gameData.Get<CultureTypeSettings>(_gs.ch).Get(_city.CultureTypeId);

            if (culture == null)
            {
                StartClose();
                return;
            }

            _uiService.SetText(ScreenHeader, _city.Name + "(" + culture.Name + ")");

            _assetService.LoadAssetInto(PanelAnchor, AssetCategoryNames.Cultures, CultureAssetNames.CityPanel,
                OnLoadCityAssets, GetToken(), _city, culture.Art);

            await Task.CompletedTask;
        }
        private void OnLoadCityAssets(GameObject go, City data, CancellationToken token)
        {
            if (go == null)
            {
                StartClose();
                return;
            }

            _panel = _clientEntityService.GetComponent<TraderCityPanel>(go);

            if (_panel == null)
            {
                StartClose();
                return;
            }
            _panel.SetData(data);
        }
    }
}


