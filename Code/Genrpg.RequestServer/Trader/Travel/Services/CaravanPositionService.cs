using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Trader.Travel.Entities;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Cities.WebApi;
using Genrpg.Shared.Trader.Roads.Settings;
using Genrpg.Shared.Trader.Roads.WebApi;

namespace Genrpg.RequestServer.Trader.Travel.Services
{

    public interface ICaravanPositionService : IInjectable
    {
        Task<EnterCityResponse> EnterCity(WebContext context, EnterCityArgs args);

        Task<EnterRoadResponse> EnterRoad(WebContext context, EnterRoadArgs args);

        Task<TurnAroundResponse> TurnAround(WebContext context, TurnAroundArgs args);

    }

    public class CaravanPositionService : ICaravanPositionService
    {
        private IGameData _gameData = null;

        public async Task<EnterCityResponse> EnterCity(WebContext context, EnterCityArgs args)
        {
            EnterCityResponse response = new EnterCityResponse()
            {

            };

            City city = _gameData.Get<CitySettings>(context.user).Get(args.CityId);
            if (args.CityId < 1)
            {
                response.ErrorMessage = "That city doesn't exist!";
                return response;
            }

            CoreUserData userData = await context.GetAsync<CoreUserData>();

            CaravanPosition position = userData.GetPosition();

            if (position.CityId == args.CityId)
            {
                response.ErrorMessage = "You are in that city!";
                return response;
            }

            if (!args.Force)
            {
                if (position.TargetCityId != args.CityId)
                {
                    response.ErrorMessage = "You aren't heading to that city!";
                    return response;
                }

                Road road = _gameData.Get<RoadSettings>(context.user).Get(position.RoadId);

                if (road == null)
                {
                    response.ErrorMessage = "You aren't on the trail!";
                    return response;
                }

                if (road.EndCityId != args.CityId && road.StartCityId != args.CityId)
                {
                    response.ErrorMessage = "That road doesn't connect to that city!";
                    return response;
                }

                if (position.DistanceTravelled < road.Distance)
                {
                    response.ErrorMessage = "You haven't arrived there yet!";
                    return response;
                }
            }

            userData.CityId = args.CityId;
            userData.RoadId = 0;
            userData.Dist = 0;

            response.CityId = userData.CityId;
            response.Success = true;

            return response;
        }

        public async Task<EnterRoadResponse> EnterRoad(WebContext context, EnterRoadArgs args)
        {
            EnterRoadResponse response = new EnterRoadResponse()
            {

            };

            Road road = _gameData.Get<RoadSettings>(context.user).Get(args.RoadId);

            if (road == null)
            {
                response.ErrorMessage = "That trail doesn't exist!";
                return response;
            }


            CoreUserData userData = await context.GetAsync<CoreUserData>();

            CaravanPosition position = userData.GetPosition();

            if (!args.Force)
            {
                if (position.OnRoad())
                {
                    response.ErrorMessage = "You're already on the trail!";
                    return response;
                }

                if (road.StartCityId != position.CityId && road.EndCityId != position.CityId)
                {
                    response.ErrorMessage = "That trail doesn't connect to your current city!";
                    return response;
                }
            }

            long otherCityId = road.GetCityIdOnOtherEnd(position.CityId);

            userData.RoadId = road.IdKey;
            userData.CityId = otherCityId;
            userData.Dist = 0;


            response.TargetCityId = userData.CityId;
            response.RoadId = userData.RoadId;
            response.DistanceTravelled = userData.Dist;

            return response;
        }

        public async Task<TurnAroundResponse> TurnAround(WebContext context, TurnAroundArgs args)
        {
            TurnAroundResponse response = new TurnAroundResponse()
            {

            };

            CoreUserData userData = await context.GetAsync<CoreUserData>();

            CaravanPosition pos = userData.GetPosition();

            Road road = _gameData.Get<RoadSettings>(context.user).Get(pos.RoadId);

            if (road == null)
            {
                response.ErrorMessage = "You aren't on a trail!";
                return response;
            }

            userData.CityId = road.GetCityIdOnOtherEnd(pos.TargetCityId);
            userData.Dist = (long)(road.Distance - userData.Dist);

            response.RoadId = road.IdKey;
            response.TargetCityId = userData.CityId;
            response.DistanceTravelled = userData.Dist;
            response.Success = true;

            return response;
        }

    }
}
