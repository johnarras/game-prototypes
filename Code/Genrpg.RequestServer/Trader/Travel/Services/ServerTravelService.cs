using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Services;
using Genrpg.RequestServer.Trader.Travel.Entities;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Roads.Settings;
using Genrpg.Shared.Trader.Travel.Entities;
using Genrpg.Shared.Trader.Travel.Settings;
using Genrpg.Shared.Trader.Travel.WebApi;
using Genrpg.Shared.Utils;

namespace Genrpg.RequestServer.Trader.Travel.Services
{
    public interface IServerTravelService : IInjectable
    {
        Task<TravelResponse> Travel(WebContext context, TravelArgs args);
    }

    public class ServerTravelService : IServerTravelService
    {
        private ICaravanService _caravanService = null;
        private IGameData _gameData = null;
        private IWebRewardService _rewardService = null;

        public async Task<TravelResponse> Travel(WebContext context, TravelArgs args)
        {
            TravelResponse response = new TravelResponse();

            CoreUserData userData = await context.GetAsync<CoreUserData>();

            CaravanPosition position = userData.GetPosition();

            if (position.CityId > 0)
            {
                response.ErrorMessage = "You are in a city!";
                return response;
            }

            Road road = _gameData.Get<RoadSettings>(context.user).Get(position.RoadId);

            if (road == null)
            {
                response.ErrorMessage = "You aren't on a trail!";
                return response;
            }

            if (userData.Dist >= road.Distance)
            {
                response.ErrorMessage = "You are at the next city!";
                return response;
            }

            CaravanTravelInfo travelInfo = _caravanService.GetTravelInfo(userData);

            TravelStatus status = new TravelStatus()
            {
                CurrentDistanceAlongRoad = userData.Dist,
                RoadDistance = road.Distance,
                TotalDistanceTravelled = 0,
                TravelInfo = travelInfo,
                TargetCityId = position.TargetCityId,
                Response = response,
                IsFree = args.IsFree,
            };


            if (!args.IsFree && travelInfo.TotalCost > userData.Currencies.Get(CoreCurrencyTypes.Food))
            {
                response.ErrorMessage = "You don't have enough food to travel this far!";
                return response;
            }
            for (int d = 0; d < travelInfo.Days; d++)
            {
                if (!await TravelOneDay(context, status))
                {
                    break;
                }
            }

            RewardParams rp = new RewardParams();

            foreach (TravelDay tday in status.Response.Days)
            {
                await _rewardService.GiveRewardsAsync(context, tday.TravelRewards, rp);
            }

            userData.Day += status.TravelDays;
            userData.Dist = status.CurrentDistanceAlongRoad;
            response.RoadId = road.IdKey;
            response.TargetCityId = position.TargetCityId;
            response.TotalCost = travelInfo.TotalCost;
            response.TotalDistanceTravelled = status.TotalDistanceTravelled;
            response.DistanceAlongRoad = status.CurrentDistanceAlongRoad;
            response.DistanceLeft = status.RoadDistance - status.CurrentDistanceAlongRoad;
            response.EndDay = userData.Day;

            return response;
        }

        private async Task<bool> TravelOneDay(WebContext context, TravelStatus status)
        {
            long distanceLeft = status.RoadDistance - status.CurrentDistanceAlongRoad;

            if (distanceLeft < 1)
            {
                status.Response.Messages.Add("You have arrived!");
                return false;
            }

            CoreUserData userData = await context.GetAsync<CoreUserData>();

            if (!status.IsFree)
            {
                if (userData.Currencies.Get(CoreCurrencyTypes.Food) < status.TravelInfo.CostPerDay)
                {
                    status.Response.Messages.Add("You ran out of food.");
                    return false;
                }
                userData.Currencies.Add(CoreCurrencyTypes.Food, -status.TravelInfo.CostPerDay);
            }

            TravelDay day = new TravelDay();
            status.Response.Days.Add(day);
            for (int i = 0; i < status.TravelInfo.DiceDistancePerDay; i++)
            {
                day.RolledDistances.Add(MathUtils.IntRange(1, 6, context.rand));
            }
            day.BonusDistance = status.TravelInfo.BonusDistancePerDay;


            long distanceToday = day.RolledDistances.Sum(x => x) + day.BonusDistance;

            if (distanceToday > distanceLeft)
            {
                distanceToday = distanceLeft;
            }

            long endDistance = status.CurrentDistanceAlongRoad + distanceToday;

            day.TotalDistance = distanceToday;
            day.EndDistance = endDistance;
            status.TravelDays++;
            status.TotalDistanceTravelled += distanceToday;
            status.CurrentDistanceAlongRoad = endDistance;

            TravelRewardSettings rewardSettings = _gameData.Get<TravelRewardSettings>(context.user);

            IReadOnlyList<TravelReward> travelRewards = rewardSettings.GetData();

            for (int dist = 0; dist < distanceToday; dist++)
            {
                if (context.rand.NextDouble() > rewardSettings.TravelRewardChance)
                {
                    continue;
                }

                TravelReward chosenReward = RandomUtils.GetRandomElement(travelRewards, context.rand);

                Reward rew = day.TravelRewards.FirstOrDefault(x => x.EntityTypeId == chosenReward.EntityTypeId && x.EntityId == chosenReward.EntityId);

                if (rew == null)
                {
                    rew = new Reward() { EntityTypeId = chosenReward.EntityTypeId, EntityId = chosenReward.EntityId };
                    day.TravelRewards.Add(rew);
                    rew.Quantity += MathUtils.LongRange(chosenReward.MinQuantity, chosenReward.MaxQuantity, context.rand);
                }
            }

            return true;
        }
    }
}
