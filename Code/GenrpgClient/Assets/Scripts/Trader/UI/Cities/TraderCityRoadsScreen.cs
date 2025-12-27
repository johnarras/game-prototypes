using Assets.Scripts.UI.ScreenSystem;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Cities.WebApi;
using Genrpg.Shared.Trader.Roads.Settings;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.UI.Cities
{
    public class TraderCityRoadsScreen : TypedArgScreen<TraderCityRoadsScreenArgs>
    {

        private IClientWebService _webService = null;

        public GText HeaderText;
        public GameObject RoadRowAnchor;
        public TraderRoadRowUI RowPrefab;

        public GameObject EnterCityParent;
        public GText EnterCityText;
        public GButton EnterCityButton;

        private List<TraderRoadRowUI> _roads = new List<TraderRoadRowUI>();

        private City _city = null;
        protected override async Task OnStartOpen(TraderCityRoadsScreenArgs data, CancellationToken token)
        {

            CoreUserData userData = _gs.ch.Get<CoreUserData>();

            _uiService.SetButton(EnterCityButton, GetName(), ClickEnterCity);

            bool canEnterCity = false;
            long cityId = 0;

            if (data != null)
            {
                cityId = data.CityId;
                canEnterCity = data.CanEnterCity;
            }
            else
            {
                CaravanPosition position = userData.GetPosition();
                if (position.CityId < 1)
                {
                    StartClose();
                    return;
                }
                cityId = position.CityId;
            }

            _city = _gameData.Get<CitySettings>(_gs.ch).Get(cityId);

            if (_city == null)
            {
                StartClose();
                return;
            }

            _uiService.SetText(HeaderText, _city.Name);

            ShowRoads(_city);

            ShowEnterCity(_city, canEnterCity);

            await Task.CompletedTask;
        }

        private void ShowEnterCity(City city, bool canEnterCity)
        {
            _clientEntityService.SetActive(EnterCityParent, canEnterCity);

            if (canEnterCity)
            {
                _uiService.SetText(EnterCityText, "You have arrived at " + city.Name);
            }
        }

        private void ClickEnterCity()
        {
            if (_city != null)
            {
                _webService.SendClientUserWebRequest(new EnterCityRequest() { CityId = _city.IdKey }, GetToken());
            }
        }

        private void ShowRoads(City city)
        {
            if (city == null)
            {
                StartClose();
                return;
            }

            RoadSettings roadSettings = _gameData.Get<RoadSettings>(_gs.ch);

            List<Road> roads = new List<Road>();

            foreach (CityRoad cr in city.Roads)
            {
                Road road = roadSettings.Get(cr.RoadId);

                if (road != null)
                {
                    roads.Add(road);
                }
            }

            if (roads.Count < 1)
            {
                StartClose();
                return;
            }

            _clientEntityService.DestroyAllChildren(RoadRowAnchor);
            _roads.Clear();

            foreach (Road road in roads)
            {

                TraderRoadArgs args = new TraderRoadArgs()
                {
                    CanTravel = true,
                    FromCityId = city.IdKey,
                    Road = road,
                };

                TraderRoadRowUI ui = _clientEntityService.FullInstantiate(RowPrefab);

                _clientEntityService.AddToParent(ui, RoadRowAnchor);

                ui.SetData(args);
            }
        }
    }
}
