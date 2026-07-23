using OxDb.Client.Awaitables;
using OxDb.Client.ClientEvents.UI;
using OxDb.Client.Doobers.Events;
using OxDb.Client.DynamicUI.Services;
using OxDb.Client.FloatingText.ClientEvents;
using OxDb.Client.Networking.Services;
using OxDb.Client.Rewards.Services;
using OxDb.Client.Trader.ClientEvents;
using OxDb.Client.Trader.Encounters.Services;
using OxDb.Client.Trader.HUD.ClientEvents;
using OxDb.Client.Trader.Levels.Services;
using OxDb.Client.Trader.Travel.ClientEvents;
using OxDb.Client.Trader.UI.Cities;
using OxDb.Client.UI.Interfaces;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Attributes.PlayerData;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.LevelTracks.WebApi;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.PlayerData;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Constants;
using OxDb.SharedGame.Trader.Travel.Entities;
using OxDb.SharedGame.Trader.Travel.Services;
using OxDb.SharedGame.Trader.Travel.WebApi;
using OxDb.SharedGame.UI.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Trader.Travel.Services
{
    public interface IClientTravelService : IInitializable
    {
        ValueTask ClickTravelButton(CancellationToken token);

    }


    public class ClientTravelService : IClientTravelService
    {
        private IClientGameState _gs = null;
        private ITravelService _travelService = null;
        private IDispatcher _dispatcher = null;
        private IClientWebRequestService _webService = null;
        private IAwaitableService _awaitableService = null;
        private IRewardService _rewardService = null;
        private ILogService _logService = null;
        private ICaravanService _caravanService = null;
        private IUIService _uiService = null;
        private IAttributeService _attributeService = null;
        private ITraderLevelService _traderLevelService = null;
        private IDynamicUIService _dynamicUIService = null;
        private ITravelEncounterService _encounterService = null;
        private ICalcAttributeService _calcAttributeService = null;
        private IAnalyticsService _analyticsService = null;

        private CancellationToken _token;

        public const int VisualFramesPerUnitOfDistance = 5;

        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _dispatcher.AddListener<TravelResponse>(OnTravelResponse, token);
            await Task.CompletedTask;
        }

        public async ValueTask ClickTravelButton(CancellationToken token)
        {
            CoreData coreData = _gs.ch.Get<CoreData>();
            CaravanPosition pos = await _caravanService.GetPosition(_gs.ch);

            if (pos.GetCurrentCity() != null && pos.TargetCity == pos.PositionCity)
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
                    _webService.SendMainServerRequest(req, _token);
                }
            }
        }

        private void OnTravelResponse(TravelResponse response)
        {
            _awaitableService.ForgetAwaitable(ShowTravelNow(response, _token));
        }

        private void TrackTravelRewards(TravelResponse response)
        {
            AccumulatedRewards accumulatedRewards = new AccumulatedRewards();

            for (int day = 0; day < response.Days.Count; day++)
            {
                TravelDay td = response.Days[day];

                int rollCount = td.Vars[DayVars.DiceCount];
                for (int i = 0; i < td.Currencies.Count(); i++)
                {
                    if (td.Currencies[i] != 0)
                    {
                        accumulatedRewards.AddReward(EntityTypes.CoreCurrency, i, td.Currencies[i], RewardSources.TravelSpend);
                    }
                }

                int idx = DayVars.DiceCount + rollCount + 1;

                int rewardCount = td.Vars[idx];

                idx++;
                for (int r = 0; r < rewardCount; r++)
                {
                    accumulatedRewards.AddReward(td.Vars[idx++], td.Vars[idx++], td.Vars[idx++], RewardSources.TravelReward);
                }

                if (td.EncounterResult != null)
                {
                    accumulatedRewards.AddLists(td.EncounterResult.RewardLists);
                }

                if (td.ExpResponse != null)
                {
                    accumulatedRewards.AddReward(EntityTypes.CoreCurrency, CoreCurrencyTypes.Exp, td.ExpResponse.ExpGained, RewardSources.TravelReward);
                    foreach (LevelGained levelGained in td.ExpResponse.LevelsGained)
                    {
                        if (levelGained.Rewards != null)
                        {
                            accumulatedRewards.AddRewards(levelGained.Rewards, RewardSources.LevelUp);
                        }
                    }
                }
            }

            _analyticsService.TrackAccumulatedRewards(accumulatedRewards);
        }

        private async Awaitable ShowTravelNow(TravelResponse response, CancellationToken token)
        {
            try
            {
                CoreData coreData = _gs.ch.Get<CoreData>();

                CaravanData caravanData = _gs.ch.Get<CaravanData>();
                AttributesData AttributesData = _gs.ch.Get<AttributesData>();

                CaravanPosition pos = await _caravanService.GetPosition(_gs.ch);

                if (pos.TotalDistanceToTarget < 1)
                {
                    pos.TotalDistanceToTarget = 1;
                }

                long startDist = pos.DistanceGone;

                TrackTravelRewards(response);

                for (int day = 0; day < response.Days.Count; day++)
                {
                    TravelDay td = response.Days[day];

                    for (int i = 0; i < td.Currencies.Count(); i++)
                    {
                        if (td.Currencies[i] != 0)
                        {
                            _ = _rewardService.GiveReward(_gs.ch, EntityTypes.CoreCurrency, i, td.Currencies[i], RewardSources.TravelSpend, null, 0,
                                new ClientRewardParams(false, true, true));
                        }
                    }

                    _ = _attributeService.AddDebuffDaysPlayed(_gs.ch, 1);
                    _dispatcher.Dispatch(td);
                    long distanceGoneToday = td.Vars[DayVars.EndDistance] - startDist;
                    double currDist = startDist;

                    int rollCount = td.Vars[DayVars.DiceCount];
                    List<int> rolledDistances = new List<int>();

                    for (int rindex = 0; rindex < rollCount; rindex++)
                    {
                        rolledDistances.Add(td.Vars[DayVars.DiceCount + 1 + rindex]);
                    }

                    _dispatcher.Dispatch(new ShowTraderDiceRoll()
                    {
                        RolledDistances = rolledDistances,
                        BonusDistance = td.Vars[DayVars.BonusDistance],
                        TotalDistance = td.Vars[DayVars.TotalDistance]
                    });

                    List<DooberArgs> rewardDoobers = new List<DooberArgs>();
                    List<DooberArgs> expDoobers = new List<DooberArgs>();

                    int travelRewardCountIndex = DayVars.DiceCount + rollCount + 1;

                    int rewardCount = td.Vars[travelRewardCountIndex];

                    List<Reward> rewards = new List<Reward>();

                    travelRewardCountIndex++;
                    for (int r = 0; r < rewardCount; r++)
                    {

                        rewards.Add(new Reward()
                        {
                            EntityTypeId = td.Vars[travelRewardCountIndex++],
                            EntityId = td.Vars[travelRewardCountIndex++],
                            Quantity = td.Vars[travelRewardCountIndex++],
                        });
                    }

                    for (int rewardIndex = 0; rewardIndex < rewards.Count; rewardIndex++)
                    {
                        Reward rew = rewards[rewardIndex];
                        for (int q = 0; q < rew.Quantity; q++)
                        {
                            rewardDoobers.Add(_dynamicUIService.CheckoutSimpleEntityDooberArgs(rew.EntityTypeId, rew.EntityId, 1));
                        }
                        _ = _rewardService.GiveReward(_gs.ch, rew, RewardSources.TravelReward, new ClientRewardParams(false, false, true));

                    }

                    int expGained = td.Vars[DayVars.Exp];

                    for (int exp = 0; exp < expGained; exp++)
                    {
                        expDoobers.Add(_dynamicUIService.CheckoutSimpleEntityDooberArgs(EntityTypes.CoreCurrency, CoreCurrencyTypes.Exp, 1));
                    }

                    rewardDoobers = rewardDoobers.OrderBy(x => Guid.NewGuid()).ToList();

                    float x = 0;
                    float z = 0;

                    long totalTicks = distanceGoneToday * VisualFramesPerUnitOfDistance;
                    long ticksLeft = totalTicks;
                    for (int distToday = 0; distToday < distanceGoneToday; distToday++)
                    {
                        double viewTickDist = currDist;
                        for (int viewTicks = 0; viewTicks < VisualFramesPerUnitOfDistance; viewTicks++)
                        {
                            viewTickDist = currDist + ((viewTicks + 1) * 1.0f / VisualFramesPerUnitOfDistance);

                            _dispatcher.Dispatch(new ShowTraderMapPosition() { Pos = pos, DistanceGone = viewTickDist });
                            ticksLeft--;

                            ShowMoreDoobers(rewardDoobers, ticksLeft);
                            ShowMoreDoobers(expDoobers, ticksLeft);

                            await Awaitable.NextFrameAsync(token);
                        }
                        currDist = viewTickDist;
                    }

                    pos = await _caravanService.GetPosition(_gs.ch);


                    if (td.Vars[DayVars.EndFlags] != coreData.Vars[TraderVars.Flags])
                    {
                        coreData.Vars[TraderVars.Flags] = td.Vars[DayVars.EndFlags];
                        _ = _calcAttributeService.CalcBuffs(_gs.ch);
                    }
                    _dispatcher.Dispatch(new UpdateTraderHUD());
                    bool isWater = _travelService.IsWater(x, z);

                    startDist = td.Vars[DayVars.EndDistance];

                    _awaitableService.ForgetAwaitable(_encounterService.ShowEncounterResult(td.EncounterResult));

                    await _traderLevelService.ShowLevelGain(td.ExpResponse, false, RewardSources.TravelReward);

                    coreData.Vars[TraderVars.PlayCount] = td.Vars[DayVars.Day];
                    if (day < response.Days.Count - 1)
                    {
                        _dispatcher.Dispatch(new UpdateTraderHUD());
                    }
                }

                coreData.Vars[TraderVars.DistanceGone] = response.DistanceAlongRoad;
                coreData.Vars[TraderVars.PlayCount] = response.EndDay;


                bool atEndOfRoad = coreData.Vars[TraderVars.DistanceGone] >= coreData.Vars[TraderVars.TotalDistanceToTarget];
                _dispatcher.Dispatch(new UpdateTraderHUD() { FullRefresh = atEndOfRoad });

                if (atEndOfRoad)
                {
                    pos = await _caravanService.GetPosition(_gs.ch);
                    if (pos.GetCurrentCity() != null)
                    {
                        _dispatcher.Dispatch(new OpenScreen(ScreenNames.TraderCity));
                    }
                }

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

        private void ShowMoreDoobers(List<DooberArgs> doobers, long ticksLeft)
        {
            if (doobers.Count > 0)
            {
                int currentDooberCount = 0;
                if (ticksLeft > 0)
                {
                    if (doobers.Count < ticksLeft &&
                            _gs.Rand.Next() % ticksLeft < doobers.Count)
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
