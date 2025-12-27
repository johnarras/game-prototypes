using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents.Entities;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Doobers.Events;
using Assets.Scripts.Trader.UI.Cities;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Roads.Settings;
using Genrpg.Shared.Trader.Travel.Entities;
using Genrpg.Shared.Trader.Travel.Services;
using Genrpg.Shared.Trader.Travel.WebApi;
using Genrpg.Shared.UI.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.Travel.Services
{
    public interface IClientTravelService : IInitializable
    {
        void ClickTravelButton();

    }


    public class ClientTravelService : IClientTravelService
    {
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private ITravelService _travelService = null;
        private IScreenService _screenService = null;
        private IDispatcher _dispatcher = null;
        private IClientWebService _webService = null;
        private IAwaitableService _awaitableService = null;
        private IRewardService _rewardService = null;
        private IClientRandom _rand = null;
        private ILogService _logService = null;

        private CancellationToken _token;

        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _dispatcher.AddListener<TravelResponse>(OnTravelResponse, token);
        }

        private bool _travellingNow = false;
        public void ClickTravelButton()
        {
            if (_travellingNow)
            {
                return;
            }
            CoreUserData userData = _gs.ch.Get<CoreUserData>();
            CaravanPosition pos = userData.GetPosition();

            if (pos.CityId > 0)
            {
                TraderCityRoadsScreenArgs args = new TraderCityRoadsScreenArgs() { CityId = pos.CityId };

                _dispatcher.Dispatch(new OpenScreen(ScreenNames.TraderCityRoads, args));
            }
            else
            {
                Road road = _gameData.Get<RoadSettings>(_gs.ch).Get(pos.RoadId);

                if (userData.Dist >= road.Distance)
                {
                    TraderCityRoadsScreenArgs args = new TraderCityRoadsScreenArgs() { CityId = pos.TargetCityId, CanEnterCity = true };

                    _dispatcher.Dispatch(new OpenScreen(ScreenNames.TraderCityRoads, args));
                }
                else
                {

                    _travellingNow = true;
                    TravelRequest req = new TravelRequest();

                    _webService.SendClientUserWebRequest(req, _token);
                }
            }
        }

        private void OnTravelResponse(TravelResponse response)
        {
            if (!_travellingNow)
            {
                return;
            }


            _awaitableService.ForgetAwaitable(ShowTravelNow(response, _token));
        }

        private async Awaitable ShowTravelNow(TravelResponse response, CancellationToken token)
        {
            try
            {
                CoreUserData userData = _gs.ch.Get<CoreUserData>();
                if (response.TotalCost > 0)
                {
                    _rewardService.GiveReward(_rand, _gs.ch, EntityTypes.CoreCurrency, CoreCurrencyTypes.Food, -response.TotalCost, null, new RewardParams());
                    _dispatcher.Dispatch(new ReplaceEntityModel() { EntityTypeId = EntityTypes.CoreCurrency, EntityId = CoreCurrencyTypes.Food });
                }

                for (int d = 0; d < response.Days.Count; d++)
                {
                    TravelDay td = response.Days[d];
                    for (int r = 0; r < td.TravelRewards.Count; r++)
                    {
                        Reward rew = td.TravelRewards[r];
                        for (int q = 0; q < rew.Quantity; q++)
                        {
                            _dispatcher.Dispatch(new ShowDooberEvent() { EntityTypeId = rew.EntityTypeId, EntityId = rew.EntityId, Quantity = 1, StartsInUI = true });
                            await Awaitable.NextFrameAsync(token);
                        }
                        _rewardService.GiveReward(_rand, _gs.ch, rew, new RewardParams());
                    }
                }

                userData.Dist = response.DistanceAlongRoad;
                userData.Day = response.EndDay;
                _logService.Info("NewDist: " + userData.Dist + " Left: " + response.DistanceLeft + " Went: " + response.TotalDistanceTravelled);

                foreach (string msg in response.Messages)
                {
                    _dispatcher.Dispatch(new ShowFloatingText(msg));
                }
            }
            catch (Exception e)
            {
                _logService.Exception(e, "ShowTravel");
            }
            _travellingNow = false;
        }
    }
}
