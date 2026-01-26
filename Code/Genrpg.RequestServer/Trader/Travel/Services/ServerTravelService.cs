using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Services;
using Genrpg.RequestServer.Trader.Travel.Entities;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
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
        private IWebRewardService _rewardService = null;
        private ITravelService _travelService = null;
        private IServerCaravanService _serverCaravanService = null;
        private ITraderMapService _traderMapService = null;
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

            TravelSettings travelSettings = _gameData.Get<TravelSettings>(context.core);

            TraderFlagSettings flagSettings = _gameData.Get<TraderFlagSettings>(context.core);

            if (position.GetCurrentCity() != null)
            {
                response.ErrorMessage = "You are in a city!";
                return response;
            }

            if (coreData.Vars[TraderVars.DistanceGone] >= coreData.Vars[TraderVars.DistanceToTarget])
            {
                response.ErrorMessage = "You already reached your target!";
                return response;
            }

            CaravanTravelInfo travelInfo = _caravanService.GetTravelInfo(coreData);

            TravelStatus status = new TravelStatus()
            {
                DistanceGone = position.DistanceGone,
                DistanceToTarget = position.DistanceToTarget,
                TotalDistanceTravelled = 0,
                TargetCityId = position.GetTargetCityId(),
                Response = response,
                IsFree = args.IsFree,
            };

            for (int d = 0; d < travelInfo.Days; d++)
            {
                if (!await TravelOneDay(context, coreData, status, flagSettings))
                {
                    break;
                }
            }

            RewardParams rp = new RewardParams();

            foreach (TravelDay tday in status.Response.Days)
            {
                await _rewardService.GiveRewardsAsync(context, tday.TravelRewards, rp);
            }

            coreData.Vars.Add(TraderVars.PlayCount, status.TravelDays);
            coreData.Vars[TraderVars.DistanceGone] = status.DistanceGone;
            response.TargetCityId = position.GetTargetCityId();
            response.TotalCost = status.Response.Days.Sum(x => x.RationsCost);
            response.TotalDistanceTravelled = status.TotalDistanceTravelled;
            response.DistanceAlongRoad = status.DistanceGone;
            response.DistanceLeft = status.DistanceToTarget - status.DistanceGone;
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
            int distanceLeft = status.DistanceToTarget - status.DistanceGone;

            if (distanceLeft < 1)
            {
                status.Response.Messages.Add("You have arrived!");
                return false;
            }

            TravelDay day = new TravelDay();
            if (!status.IsFree)
            {
                int rationsCost = Math.Max(1, coreData.Vars[TraderVars.RationsCost]);
                if (coreData.Currencies[CoreCurrencyTypes.Rations] < rationsCost)
                {
                    status.Response.Messages.Add("You ran out of rations.");
                    return false;
                }
                coreData.Currencies.Add(CoreCurrencyTypes.Rations, -rationsCost);
                day.RationsCost = rationsCost;
            }

            status.Response.Days.Add(day);
            for (int i = 0; i < coreData.Vars[TraderVars.DiceSpeed]; i++)
            {
                day.RolledDistances.Add(MathUtil.IntRange(1, 6, context.rand));
            }
            day.BonusDistance = coreData.Vars[TraderVars.BonusSpeed];

            int distanceToday = day.RolledDistances.Sum(x => x) + day.BonusDistance;

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

            day.TotalDistance = distanceToday;
            day.EndDistance = endDistance;
            status.TravelDays++;
            day.Day = coreData.Vars[TraderVars.PlayCount] + status.TravelDays;
            status.TotalDistanceTravelled += distanceToday;
            status.DistanceGone = endDistance;

            TravelRewardSettings rewardSettings = _gameData.Get<TravelRewardSettings>(context.core);


            IReadOnlyList<TravelReward> travelRewards = rewardSettings.GetData();

            double forageChance = coreData.Vars[TraderVars.ForageChance] * 0.01f * coreData.Vars[TraderVars.RationsCost];

            int guaranteedForage = (int)(forageChance);
            double forageRemainder = forageChance - guaranteedForage;

            for (int dist = 0; dist < distanceToday; dist++)
            {
                int maxTimes = guaranteedForage;

                if (context.rand.NextDouble() < forageRemainder)
                {
                    maxTimes++;
                }

                for (int times = 0; times < maxTimes; times++)
                {
                    TravelReward chosenReward = RandomUtils.GetRandomElement(travelRewards, context.rand);

                    Reward rew = day.TravelRewards.FirstOrDefault(x => x.EntityTypeId == chosenReward.EntityTypeId && x.EntityId == chosenReward.EntityId);

                    if (rew == null)
                    {
                        rew = new Reward() { EntityTypeId = chosenReward.EntityTypeId, EntityId = chosenReward.EntityId };
                        day.TravelRewards.Add(rew);
                        rew.Quantity += MathUtil.LongRange(chosenReward.MinQuantity, chosenReward.MaxQuantity, context.rand);
                    }
                }
            }

            if (day.TravelRewards.Count < 1)
            {
                day.TravelRewards.Add(new Reward() { EntityTypeId = EntityTypes.CoreCurrency, EntityId = CoreCurrencyTypes.Coins, Quantity = 1 });
            }

            MyPointF endPoint = _traderMapService.GetMapCoordinate(coreData.Vars[TraderVars.FromX],
                coreData.Vars[TraderVars.FromY],
                coreData.Vars[TraderVars.ToX],
                coreData.Vars[TraderVars.ToY],
                day.EndDistance,
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

            day.EndDiceSpeed = coreData.Vars[TraderVars.DiceSpeed];
            day.EndBonusSpeed = coreData.Vars[TraderVars.BonusSpeed];
            day.EndFlags = coreData.Vars[TraderVars.Flags];
            await Task.CompletedTask;
            return true;
        }
    }
}
