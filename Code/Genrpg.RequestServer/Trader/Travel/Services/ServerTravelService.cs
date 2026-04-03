using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.LevelTrack.Services;
using Genrpg.RequestServer.Trader.Encounters.Services;
using Genrpg.RequestServer.Trader.Stats.Services;
using Genrpg.RequestServer.Trader.Travel.Entities;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Flags.Constants;
using Genrpg.Shared.Trader.Flags.Settings;
using Genrpg.Shared.Trader.Maps.Services;
using Genrpg.Shared.Trader.Travel.Entities;
using Genrpg.Shared.Trader.Travel.Services;
using Genrpg.Shared.Trader.Travel.Settings;
using Genrpg.Shared.Trader.Travel.WebApi;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;

namespace Genrpg.RequestServer.Trader.Travel.Services
{
    public interface IServerTravelService : IInitializable
    {
        Task<TravelResponse> Travel(WebContext context, TravelArgs args);
    }

    public class ServerTravelService : IServerTravelService
    {
        private ICaravanService _caravanService = null;
        private IGameData _gameData = null;
        private IRewardService _rewardService = null;
        private ITravelService _travelService = null;
        private IServerCaravanService _serverCaravanService = null;
        private ITraderMapService _traderMapService = null;
        private IServerGameStatService _serverStatService = null;
        private ITravelEncounterService _encounterService = null;
        private IServerLevelTrackService _levelService = null;
        public async Task Initialize(CancellationToken token)
        {
            _travelService.SetTerrainMap(File.ReadAllBytes("AppData/WorldMapColorIndexes.bytes"));
            await Task.CompletedTask;
        }

        public async Task<TravelResponse> Travel(WebContext context, TravelArgs args)
        {
            TravelResponse response = new TravelResponse();

            CoreData coreData = await context.GetAsync<CoreData>();

            CaravanPosition position = _caravanService.GetPosition(coreData);

            TravelSettings travelSettings = _gameData.Get<TravelSettings>(coreData);

            TraderFlagSettings flagSettings = _gameData.Get<TraderFlagSettings>(coreData);

            if (!position.IsTravelling())
            {
                response.ErrorMessage = "You are in a city!";
                return response;
            }

            if (coreData.Vars[TraderVars.DistanceGone] >= coreData.Vars[TraderVars.TotalDistanceToTarget])
            {
                response.ErrorMessage = "You already reached your target!";
                return response;
            }

            CaravanTravelInfo travelInfo = _caravanService.GetTravelInfo(coreData);

            TravelStatus status = new TravelStatus()
            {
                DistanceGone = position.DistanceGone,
                TotalDistanceToTarget = position.TotalDistanceToTarget,
                TotalDistanceTravelledToday = 0,
                TargetCityId = position.GetTargetCityId(),
                Response = response,
                TravelInfo = travelInfo,    
            };

            if (travelInfo.DiceSpeed < 1)
            {
                response.ErrorMessage = "You can't move!";
                return response;
            }

            for (int d = 0; d < travelInfo.Days; d++)
            {
                if (!await TravelOneDay(context, coreData, status, flagSettings))
                {
                    break;
                }
            }

            coreData.Vars.Add(TraderVars.PlayCount, status.TravelDays);
            coreData.Vars[TraderVars.DistanceGone] = status.DistanceGone;
            response.TargetCityId = position.GetTargetCityId();
            response.TotalDistanceTravelled = status.TotalDistanceTravelledToday;
            response.DistanceAlongRoad = status.DistanceGone;
            response.DistanceLeft = status.TotalDistanceToTarget - status.DistanceGone;
            response.EndDay = coreData.Vars[TraderVars.PlayCount];

            if (response.DistanceLeft < 1)
            {
                await _serverCaravanService.EnterCity(context, position.GetTargetCityId(), false);
                response.EnterCityId = position.GetTargetCityId();
            }

            return response;
        }

        private async Task<bool> TravelOneDay(WebContext context, CoreData coreData, TravelStatus status, TraderFlagSettings flagSettings)
        {
            int distanceLeft = status.TotalDistanceToTarget - status.DistanceGone;

            if (distanceLeft < 1)
            {
                City city = _gameData.Get<CitySettings>(coreData).Get(status.TargetCityId);
                status.Response.Messages.Add("You have arrived in " + city.Name + "!");
                return false;
            }

            TravelDay day = new TravelDay();

            for (int i = 0; i < coreData.TravelDayCurrencies.Count(); i++)
            {
                if (coreData.TravelDayCurrencies[i] < 0 && coreData.Currencies[i] < -coreData.TravelDayCurrencies[i])
                {
                    status.Response.Messages.Add("You ran out of resources and must stop.");
                    return false;
                }
            }

            for (int i = 0; i < coreData.TravelDayCurrencies.Count(); i++)
            {
                coreData.Currencies.Add(i, coreData.TravelDayCurrencies[i]);
            }

            day.Currencies.CopyFrom(coreData.TravelDayCurrencies);

            List<int> rolledDistances = new List<int>();
            status.Response.Days.Add(day);
            int diceSpeed = status.TravelInfo.DiceSpeed;
            int bonusDistance = status.TravelInfo.BonusSpeed;
            for (int i = 0; i < diceSpeed; i++)
            {
                rolledDistances.Add(RandUtils.IntRange(1, 6, context.rand));
            }
            day.Vars[DayVars.BonusDistance] = coreData.GetBonusSpeed();

            int distanceToday = rolledDistances.Sum(x => x) + bonusDistance;

            day.Vars[DayVars.DiceCount] = rolledDistances.Count;

            for (int r = 0; r < rolledDistances.Count; r++)
            {
                day.Vars[r + DayVars.DiceCount + 1] = rolledDistances[r];
            }

            if (distanceToday > distanceLeft)
            {
                distanceToday = distanceLeft;
            }
            // Bad things can reduce the travel speed per die, so must at least give 1 unit of distance.
            if (distanceToday < 1)
            {
                distanceToday = 1;
            }

            int endDistance = status.DistanceGone + distanceToday;

            day.Vars[DayVars.TotalDistance] = distanceToday;
            day.Vars[DayVars.EndDistance] = endDistance;
            status.TravelDays++;
            day.Vars[DayVars.Day] = coreData.Vars[TraderVars.PlayCount] + status.TravelDays;
            status.TotalDistanceTravelledToday += distanceToday;
            status.DistanceGone = endDistance;

            TravelRewardSettings rewardSettings = _gameData.Get<TravelRewardSettings>(coreData);

            IReadOnlyList<TravelReward> travelRewardOptions = rewardSettings.GetData();

            long totalCost = -coreData.TravelDayCurrencies.Data.Where(x => x < 0).Sum();
            double forageChance = coreData.Vars[TraderVars.Searching] * 0.01f + 0.01f * totalCost;

            int guaranteedForage = (int)(forageChance);
            double forageRemainder = forageChance - guaranteedForage;

            List<Reward> currentTravelRewards = new List<Reward>();

            for (int dist = 0; dist < distanceToday; dist++)
            {
                int maxTimes = guaranteedForage;

                if (context.rand.NextDouble() < forageRemainder)
                {
                    maxTimes++;
                }

                for (int times = 0; times < maxTimes; times++)
                {
                    TravelReward chosenReward = RandUtils.GetRandomElement(travelRewardOptions, context.rand);

                    Reward rew = currentTravelRewards.FirstOrDefault(x => x.EntityTypeId == chosenReward.EntityTypeId && x.EntityId == chosenReward.EntityId);

                    if (rew == null)
                    {
                        rew = new Reward() { EntityTypeId = chosenReward.EntityTypeId, EntityId = chosenReward.EntityId };
                        currentTravelRewards.Add(rew);
                    }
                    rew.Quantity += RandUtils.LongRange(chosenReward.MinQuantity, chosenReward.MaxQuantity, context.rand);
                }
            }

            if (currentTravelRewards.Count < 1)
            {
                currentTravelRewards.Add(new Reward() { EntityTypeId = EntityTypes.CoreCurrency, EntityId = CoreCurrencyTypes.Coins, Quantity = 1 });
            }



            MyPointF endPoint = _traderMapService.GetMapCoordinate(coreData.Vars[TraderVars.FromX],
                coreData.Vars[TraderVars.FromY],
                coreData.Vars[TraderVars.ToX],
                coreData.Vars[TraderVars.ToY],
                day.Vars[DayVars.EndDistance],
                status.DistanceGone
                );

            bool onWater = _travelService.IsWater(endPoint.X, endPoint.Y);

            if (onWater)
            {
                coreData.AddFlag(TraderFlags.AtSea);
            }
            else
            {
                coreData.RemoveFlag(TraderFlags.AtSea);
            }

            day.Vars[DayVars.EndFlags] = coreData.Vars[TraderVars.Flags];
            // Do this before new encounters so you don't instantly lose a debuff day.
            await _serverStatService.AddDebuffDaysPlayed(context, 1, false);

            day.EncounterResult = await _encounterService.TryEndOfTravelDayEncounter(context, status, day);

            day.ExpResponse = await _levelService.GainExp(context, day.Vars[DayVars.TotalDistance], false);

            day.Vars[DayVars.Exp] = (int)day.ExpResponse.ExpGained;

            if (day.ExpResponse.LevelsGained == null || day.ExpResponse.LevelsGained.Count < 1)
            {
                day.ExpResponse = null;
            }
            await _rewardService.GiveRewards(context, currentTravelRewards, null);

            int maxVarIndexUsed = DayVars.DiceCount + day.Vars[DayVars.DiceCount] + 1;

            day.Vars[maxVarIndexUsed] = currentTravelRewards.Count;


            maxVarIndexUsed++;
            for (int r = 0; r < currentTravelRewards.Count; r++)
            {
                day.Vars[maxVarIndexUsed++] = (int)currentTravelRewards[r].EntityTypeId;
                day.Vars[maxVarIndexUsed++] = (int)currentTravelRewards[r].EntityId;
                day.Vars[maxVarIndexUsed++] = (int)currentTravelRewards[r].Quantity;
            }

            return true;
        }
    }
}
