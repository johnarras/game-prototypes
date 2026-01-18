using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents.Entities;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Doobers.Events;
using Assets.Scripts.Trader.ClientEvents;
using Assets.Scripts.Trader.HUD.ClientEvents;
using Assets.Scripts.Trader.Travel.ClientEvents;
using Assets.Scripts.Trader.UI.Cities;
using Assets.Scripts.UI.Interfaces;
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
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Maps.Services;
using Genrpg.Shared.Trader.Travel.Entities;
using Genrpg.Shared.Trader.Travel.Services;
using Genrpg.Shared.Trader.Travel.WebApi;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Utils.Data;
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
        private ICaravanService _caravanService = null;
        private IUIService _uiService = null;
        private ITraderMapService _traderMapService = null;

        private CancellationToken _token;

        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _dispatcher.AddListener<TravelResponse>(OnTravelResponse, token);
        }

        public void ClickTravelButton()
        {
            CoreData coreData = _gs.ch.Get<CoreData>();
            CaravanPosition pos = _caravanService.GetPosition(coreData);

            if (pos.GetCurrentCity() != null)
            {
                TraderCityRoadsScreenArgs args = new TraderCityRoadsScreenArgs() { CityId = pos.GetCurrentCity().IdKey };

                _dispatcher.Dispatch(new OpenScreen(ScreenNames.TraderCityRoads, args));
            }
            else
            {
                if (coreData.Vars[TraderVars.DistanceGone] >= coreData.Vars[TraderVars.DistanceToTarget])
                {
                    TraderCityRoadsScreenArgs args = new TraderCityRoadsScreenArgs() { CityId = pos.GetTargetCityId(), CanEnterCity = true };

                    _dispatcher.Dispatch(new OpenScreen(ScreenNames.TraderCityRoads, args));

                }
                else
                {
                    TravelRequest req = new TravelRequest();

                    _uiService.IncrementButtonBlock();
                    _webService.SendClientUserWebRequest(req, _token);
                }
            }
        }

        private void OnTravelResponse(TravelResponse response)
        {
            _awaitableService.ForgetAwaitable(ShowTravelNow(response, _token));
        }

        private async Awaitable ShowTravelNow(TravelResponse response, CancellationToken token)
        {
            try
            {
                CoreData coreData = _gs.ch.Get<CoreData>();
                if (response.TotalCost > 0)
                {
                    _rewardService.GiveReward(_rand, _gs.ch, EntityTypes.CoreCurrency, CoreCurrencyTypes.Rations, -response.TotalCost, null, new RewardParams());
                    _dispatcher.Dispatch(new ReplaceEntityModel() { EntityTypeId = EntityTypes.CoreCurrency, EntityId = CoreCurrencyTypes.Rations });
                }

                CaravanPosition pos = _caravanService.GetPosition(coreData);

                if (pos.DistanceToTarget < 1)
                {
                    pos.DistanceToTarget = 1;
                }

                long startDist = pos.DistanceGone;
                RewardParams rp = new RewardParams();
                for (int d = 0; d < response.Days.Count; d++)
                {
                    TravelDay td = response.Days[d];
                    long endDist = td.EndDistance;

                    long distGone = td.EndDistance - startDist;
                    long currDist = startDist;
                    _dispatcher.Dispatch(new ShowTraderDiceRoll() { RolledDistances = td.RolledDistances, BonusDistance = td.BonusDistance, TotalDistance = td.TotalDistance });
                    for (int r = 0; r < td.TravelRewards.Count; r++)
                    {
                        Reward rew = td.TravelRewards[r];
                        for (int q = 0; q < rew.Quantity; q++)
                        {
                            _dispatcher.Dispatch(new ShowDooberEvent() { EntityTypeId = rew.EntityTypeId, EntityId = rew.EntityId, Quantity = 1, StartsInUI = true });

                        }
                        _rewardService.GiveReward(_rand, _gs.ch, rew, rp);
                    }

                    float x = 0;
                    float y = 0;
                    for (int i = 0; i < distGone; i++)
                    {
                        currDist++;
                        float distPct = 1.0f * currDist / pos.DistanceToTarget;

                        MyPointF mapCoord = _traderMapService.GetMapCoordinate(pos.FromX, pos.FromY, pos.ToX, pos.ToY, currDist, pos.DistanceToTarget);

                        x = mapCoord.X;
                        y = mapCoord.Y;

                        _dispatcher.Dispatch(new ShowTraderMapPosition() { X = x, Y = y });
                        await Awaitable.NextFrameAsync(token);
                    }

                    coreData.Vars[TraderVars.PlayCount] = td.Day;
                    coreData.Vars[TraderVars.DistanceGone] = td.EndDistance;

                    if (td.EndFlags != coreData.Vars[TraderVars.Flags])
                    {
                        coreData.Vars[TraderVars.Flags] = td.EndFlags;
                        _caravanService.UpdateTravelStats(coreData);
                    }
                    _dispatcher.Dispatch(new UpdateTraderStatusUI());
                    bool isWater = _travelService.IsWater(x, y);

                    startDist = td.EndDistance;
                }

                coreData.Vars[TraderVars.DistanceGone] = response.DistanceAlongRoad;
                coreData.Vars[TraderVars.PlayCount] = response.EndDay;

                foreach (string msg in response.Messages)
                {
                    _dispatcher.Dispatch(new ShowFloatingText(msg));
                }
            }
            catch (Exception e)
            {
                _logService.Exception(e, "ShowTravel");
            }
            finally
            {
                _uiService.DecrementButtonBlock();
            }
        }
    }
}
