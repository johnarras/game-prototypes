using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.UI.ScreenSystem;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Maps.Services;
using Genrpg.Shared.Trader.Travel.Settings;
using Genrpg.Shared.UI.Constants;
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

        public class CityDistance
        {
            public double Distance { get; set; }
            public City City { get; set; }
        }

        protected override async Task OnStartOpen(TraderCityRoadsScreenArgs data, CancellationToken token)
        {

            CoreData coreData = _gs.ch.Get<CoreData>();

            CaravanPosition pos = _caravanService.GetPosition(coreData);

            _clientEntityService.SetActive(EnterCityButtonParent, pos.GetCurrentCity() != null);
            _uiService.SetButton(EnterCityButton, GetName(), ShowCity);

            IReadOnlyList<City> allCities = _gameData.Get<CitySettings>(_gs.ch).GetData();

            List<CityDistance> distances = new List<CityDistance>();

            TravelSettings settings = _gameData.Get<TravelSettings>(_gs.ch);

            foreach (City city in allCities)
            {
                if (city.IdKey == pos.GetTargetCityId())
                {
                    continue;
                }

                double distanceToCity = _traderMapService.GetDistanceBetweenPoints(settings, pos.CurrX, pos.CurrY, city.MapPixelX, city.MapPixelY);

                if (distanceToCity == 0 || distanceToCity > settings.MaxDistanceToTarget)
                {
                    continue;
                }

                distances.Add(new CityDistance()
                {
                    City = city,
                    Distance = distanceToCity,
                });
            }

            distances = distances.OrderBy(x => x.Distance).ToList();

            List<CityDistance> forcedDistances = distances.Where(x => x.Distance < settings.MaxDistanceToTarget / 3).ToList();

            List<CityDistance> otherDistances = distances.Except(forcedDistances).ToList();

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

        private void ShowNearbyCities(CaravanPosition pos, List<CityDistance> distances)
        {

            _clientEntityService.DestroyAllChildren(RoadRowAnchor);
            _roads.Clear();

            foreach (CityDistance distance in distances)
            {
                TraderRoadArgs args = new TraderRoadArgs()
                {
                    DistanceToTarget = (int)distance.Distance,
                    TargetCity = distance.City,
                    TargetX = distance.City.MapPixelX,
                    TargetY = distance.City.MapPixelY,
                    Angle = _traderMapService.GetAngle(pos.CurrX, pos.CurrY, distance.City.MapPixelX, distance.City.MapPixelY),
                };


                TraderPathUI ui = _clientEntityService.FullInstantiate(RowPrefab);

                _clientEntityService.AddToParent(ui, RoadRowAnchor);

                ui.SetData(args);
            }
        }

        private void ShowCity()
        {
            CoreData coreData = _gs.ch.Get<CoreData>();

            CaravanPosition pos = _caravanService.GetPosition(coreData);

            if (pos.GetCurrentCity() != null)
            {
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.TraderCity, new TraderCityScreenArgs() { CityId = pos.GetCurrentCity().IdKey }));
                StartClose();
            }
        }
    }
}
