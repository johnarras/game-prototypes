using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.UI.ScreenSystem;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Cities.Settings;
using OxDb.SharedGame.Trader.Maps.Services;
using OxDb.SharedGame.Trader.Travel.Settings;
using OxDb.SharedGame.UI.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.UI.Cities
{
    public class TraderCityRoadsScreen : TypedArgScreen<TraderCityRoadsScreenArgs>
    {

        private ICaravanService _caravanService = null;
        private ITraderMapService _traderMapService = null;

        public GText HeaderText;
        public GameObject RoadRowAnchor;
        public TraderPathUI RowPrefab;

        public GameObject CompassParent;
        public GImage CompassImage;

        public GButton EnterCityButton;
        public GameObject EnterCityButtonParent;

        private List<TraderPathUI> _roads = new List<TraderPathUI>();

        protected override async Task OnStartOpen(TraderCityRoadsScreenArgs data, CancellationToken token)
        {
            CaravanPosition pos = await _caravanService.GetPosition(_gs.ch);

            _clientEntityService.SetActive(EnterCityButtonParent, pos.GetCurrentCity() != null);
            _uiService.SetButton(EnterCityButton, GetName(), ShowCity);

            IReadOnlyList<City> allCities = _gameData.Get<CitySettings>(_gs.ch).GetData();

            List<CityTravelDistance> distances = await _traderMapService.GetNearbyCities(_gs.ch);

            TravelSettings settings = _gameData.Get<TravelSettings>(_gs.ch);

            distances = distances.OrderBy(x => x.Distance).ToList();

            List<CityTravelDistance> forcedDistances = distances.Where(x => x.Distance < settings.MaxDistanceToTarget / 3).ToList();

            List<CityTravelDistance> otherDistances = distances.Except(forcedDistances).ToList();

            distances = forcedDistances;

            while (distances.Count < settings.MaxNearbyCitiesShown && otherDistances.Count > 0)
            {
                distances.Add(otherDistances[0]);
                otherDistances.RemoveAt(0);
            }

            if (pos.GetCurrentCity() != null)
            {
                _uiService.SetText(HeaderText, "In " + pos.GetCurrentCity().Name);
                _clientEntityService.SetActive(CompassParent, false);
            }
            else if (pos.TargetCity != null)
            {
                _uiService.SetText(HeaderText, "Travelling to " + pos.TargetCity.Name);
                if (CompassImage != null)
                {
                    _clientEntityService.SetActive(CompassParent, true);
                    CompassImage.transform.eulerAngles = new Vector3(0, 0, -pos.Angle);
                }
            }
            else
            {
                _uiService.SetText(HeaderText, "Travel");
                _clientEntityService.SetActive(CompassParent, false);
            }
            ShowNearbyCities(pos, distances);

            await Task.CompletedTask;
        }

        private void ShowNearbyCities(CaravanPosition pos, List<CityTravelDistance> distances)
        {

            _clientEntityService.DestroyAllChildren(RoadRowAnchor);
            _roads.Clear();

            foreach (CityTravelDistance distance in distances)
            {
                TraderRoadArgs args = new TraderRoadArgs()
                {
                    DistanceToTarget = (int)distance.Distance,
                    TargetCity = distance.City,
                    TargetX = distance.City.MapPixelX,
                    TargetY = distance.City.MapPixelZ,
                    Angle = _traderMapService.GetAngle(pos.CurrX, pos.CurrZ, distance.City.MapPixelX, distance.City.MapPixelZ),
                };


                TraderPathUI ui = _clientEntityService.FullInstantiate(RowPrefab);

                _clientEntityService.AddToParent(ui, RoadRowAnchor);

                ui.SetData(args);
            }
        }

        private async ValueTask ShowCity(CancellationToken token)
        {
            CaravanPosition pos = await _caravanService.GetPosition(_gs.ch);

            if (pos.GetCurrentCity() != null)
            {
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.TraderCity));
                StartClose();
            }
        }
    }
}
