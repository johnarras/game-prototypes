using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Doobers.Events;
using Assets.Scripts.DynamicUI.Services;
using Assets.Scripts.Rewards.Services;
using Assets.Scripts.Trader.ClientEvents;
using Assets.Scripts.Trader.Encounters.Services;
using Assets.Scripts.Trader.HUD.ClientEvents;
using Assets.Scripts.Trader.Levels.Services;
using Assets.Scripts.Trader.Travel.ClientEvents;
using Assets.Scripts.Trader.UI.Cities;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Maps.Services;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Stats.Services;
using Genrpg.Shared.Trader.Travel.Entities;
using Genrpg.Shared.Trader.Travel.Services;
using Genrpg.Shared.Trader.Travel.WebApi;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private IClientGameState _gs = null;
        private ITravelService _travelService = null;
        private IDispatcher _dispatcher = null;
        private IClientWebService _webService = null;
        private IAwaitableService _awaitableService = null;
        private IRewardService _rewardService = null;
        private IClientRandom _rand = null;
        private ILogService _logService = null;
        private ICaravanService _caravanService = null;
        private IUIService _uiService = null;
        private ITraderMapService _traderMapService = null;
        private ITraderStatService _statService = null;
        private ITraderLevelService _traderLevelService = null;
        private IDynamicUIService _dynamicUIService = null;
        private IClientEncounterService _encounterService = null;
        private IGameData _gameData = null;

        private CancellationToken _token;

        public const int FramesPerUnitOfDistance = 5;

        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _dispatcher.AddListener<TravelResponse>(OnTravelResponse, token);
            await Task.CompletedTask;
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
                if (coreData.Vars[TraderVars.DistanceGone] >= coreData.Vars[TraderVars.TotalDistanceToTarget])
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
                    _rewardService.GiveReward(_rand, _gs.ch, EntityTypes.CoreCurrency, CoreCurrencyTypes.Rations, -response.TotalCost, null,
                        new ClientRewardParams(false, true));
                }

                CaravanData caravanData = _gs.ch.Get<CaravanData>();
                TraderStatData statData = _gs.ch.Get<TraderStatData>();

                CaravanPosition pos = _caravanService.GetPosition(coreData);

                if (pos.TotalDistanceToTarget < 1)
                {
                    pos.TotalDistanceToTarget = 1;
                }

                long startDist = pos.DistanceGone;
                for (int day = 0; day < response.Days.Count; day++)
                {
                    TravelDay td = response.Days[day];

                    _statService.AddDebuffDaysPlayed(coreData, caravanData, statData, td.DebuffDaysAdded);
                    _dispatcher.Dispatch(td);
                    long distanceGoneToday = td.EndDistance - startDist;
                    double currDist = startDist;
                    _dispatcher.Dispatch(new ShowTraderDiceRoll() { RolledDistances = td.RolledDistances, BonusDistance = td.BonusDistance, TotalDistance = td.TotalDistance });

                    List<DooberArgs> rewardDoobers = new List<DooberArgs>();
                    List<DooberArgs> expDoobers = new List<DooberArgs>();
                    for (int rewardIndex = 0; rewardIndex < td.TravelRewards.Count; rewardIndex++)
                    {
                        Reward rew = td.TravelRewards[rewardIndex];
                        for (int q = 0; q < rew.Quantity; q++)
                        {
                            rewardDoobers.Add(_dynamicUIService.CheckoutSimpleEntityDooberArgs(rew.EntityTypeId, rew.EntityId, 1));
                        }
                        _rewardService.GiveReward(_rand, _gs.ch, rew, new ClientRewardParams(false, false));
                    }

                    if (td.ExpResponse != null)
                    {
                        for (int exp = 0; exp < td.ExpResponse.ExpGained; exp++)
                        {
                            expDoobers.Add(_dynamicUIService.CheckoutSimpleEntityDooberArgs(EntityTypes.CoreCurrency, CoreCurrencyTypes.Exp, 1));
                        }
                    }

                    rewardDoobers = rewardDoobers.OrderBy(x=>Guid.NewGuid()).ToList();    

                    float x = 0;
                    float y = 0;

                    long totalTicks = distanceGoneToday * FramesPerUnitOfDistance;
                    long ticksLeft = totalTicks;
                    for (int distToday = 0; distToday < distanceGoneToday; distToday++)
                    {
                        double viewDist = currDist;
                        for (int viewTicks = 0; viewTicks < FramesPerUnitOfDistance; viewTicks++)
                        {
                            viewDist = currDist + ((viewTicks + 1) * 1.0f / FramesPerUnitOfDistance);
                            MyPointF mapCoord = _traderMapService.GetMapCoordinate(pos.FromX, pos.FromY, pos.ToX, pos.ToY, viewDist, pos.TotalDistanceToTarget);

                            x = mapCoord.X;
                            y = mapCoord.Y;

                            _dispatcher.Dispatch(new ShowTraderMapPosition() { X = x, Y = y });

                            ticksLeft--;

                            ShowMoreDoobers(rewardDoobers, ticksLeft);
                            ShowMoreDoobers(expDoobers, ticksLeft);

                            await Awaitable.NextFrameAsync(token);
                        }
                        currDist = viewDist;
                    }

                    if (td.EndFlags != coreData.Vars[TraderVars.Flags])
                    {
                        coreData.Vars[TraderVars.Flags] = td.EndFlags;
                        _caravanService.UpdateTravelStats(coreData);
                    }
                    _dispatcher.Dispatch(new UpdateTraderHUD());
                    bool isWater = _travelService.IsWater(x, y);

                    startDist = td.EndDistance;

                    await _encounterService.ShowEncounterResult(td.EncounterResult);
                    await _traderLevelService.ShowLevelGain(td.ExpResponse, false);
                }

                coreData.Vars[TraderVars.DistanceGone] = response.DistanceAlongRoad;
                coreData.Vars[TraderVars.PlayCount] = response.EndDay;

                if (coreData.Vars[TraderVars.DistanceGone] >= coreData.Vars[TraderVars.TotalDistanceToTarget])
                {
                    _dispatcher.Dispatch(new UpdateTraderHUD() { FullRefresh = true });
                }

                foreach (string msg in response.Messages)
                {
                    _dispatcher.Dispatch(new ShowFloatingText(msg));
                }

                StringBuilder sb = new StringBuilder();

                IReadOnlyList<CoreCurrencyType> currencies = _gameData.Get<CoreCurrencyTypeSettings>(_gs.ch).GetData();


                sb.Append("Currencies: ");
                foreach (CoreCurrencyType ctype in currencies)
                {
                    sb.Append(ctype.Name + ": " + coreData.Currencies[ctype.IdKey] + " ");
                }
                _logService.Info(sb.ToString());
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

        private void ShowMoreDoobers(List<DooberArgs> doobers, long ticksLeft)
        {
            if (doobers.Count > 0)
            {
                int currentDooberCount = 0;
                if (ticksLeft > 0)
                {
                    if (doobers.Count < ticksLeft &&
                            _rand.Next() % ticksLeft < doobers.Count)
                    {
                        currentDooberCount = 1;
                    }
                    else if (doobers.Count > ticksLeft)
                    {
                        currentDooberCount += (int)(doobers.Count / ticksLeft);
                    }
                }
                else
                {
                    currentDooberCount = doobers.Count;
                }

                for (int dd = 0; dd < currentDooberCount && doobers.Count > 0; dd++)
                {
                    DooberArgs dooberArgs = doobers.Last();
                    doobers.Remove(dooberArgs);
                    _dynamicUIService.ShowDoober(dooberArgs);  
                }
            }
        }
    }
}
