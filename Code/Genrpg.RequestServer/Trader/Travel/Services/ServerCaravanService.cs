using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Trader.Travel.Entities;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Cities.WebApi;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Roads.Settings;
using Genrpg.Shared.Trader.Roads.WebApi;

namespace Genrpg.RequestServer.Trader.Travel.Services
{

    public interface IServerCaravanService : IInjectable
    {
        Task<EnterCityResponse> EnterCity(WebContext context, EnterCityArgs args);

        Task<EnterRoadResponse> EnterRoad(WebContext context, EnterRoadArgs args);

        Task<TurnAroundResponse> TurnAround(WebContext context, TurnAroundArgs args);
    }

    public class ServerCaravanService : IServerCaravanService
    {
        private IGameData _gameData = null;
        private ICaravanService _caravanService = null;

        public async Task<EnterCityResponse> EnterCity(WebContext context, EnterCityArgs args)
        {
            EnterCityResponse response = new EnterCityResponse()
            {

            };

            City city = _gameData.Get<CitySettings>(context.core).Get(args.CityId);
            if (args.CityId < 1)
            {
                response.ErrorMessage = "That city doesn't exist!";
                return response;
            }

            CoreData coreData = await context.GetAsync<CoreData>();

            CaravanPosition position = _caravanService.GetPosition(coreData);

            if (position.CityId == args.CityId)
            {
                response.ErrorMessage = "You are in that city!";
                return response;
            }

            if (!args.Force)
            {

                Road road = _gameData.Get<RoadSettings>(context.core).Get(position.RoadId);

                if (road == null)
                {
                    response.ErrorMessage = "You aren't on the trail!";
                    return response;
                }

                if (position.OutsideOfCityId != args.CityId)
                {
                    response.ErrorMessage = "You aren't outside of that city!";
                    return response;
                }
            }

            coreData.Vars[TraderVars.CityId] = args.CityId;
            coreData.Vars[TraderVars.RoadId] = 0;
            coreData.Vars[TraderVars.DistanceAlongRoad] = 0;

            response.CityId = coreData.Vars[TraderVars.CityId];
            response.Success = true;

            return response;
        }

        public async Task<EnterRoadResponse> EnterRoad(WebContext context, EnterRoadArgs args)
        {
            EnterRoadResponse response = new EnterRoadResponse()
            {

            };

            Road road = _gameData.Get<RoadSettings>(context.core).Get(args.RoadId);

            if (road == null)
            {
                response.ErrorMessage = "That trail doesn't exist!";
                return response;
            }


            CoreData coreData = await context.GetAsync<CoreData>();

            CaravanPosition position = _caravanService.GetPosition(coreData);

            if (!args.Force)
            {
                if (position.OnRoad()) // City is 0 in this case, so see if we are in a given city or not checking user's city. (Target city)
                {

                    if (position.OutsideOfCityId == 0)
                    {
                        response.ErrorMessage = "You're already on the trail!";
                        return response;
                    }

                    if (road.StartCityId != position.OutsideOfCityId && road.EndCityId != position.OutsideOfCityId)
                    {
                        response.ErrorMessage = "That trail doesn't connect to your current target city!";
                        return response;
                    }
                }
                else
                {
                    if (road.StartCityId != position.CityId && road.EndCityId != position.CityId)
                    {
                        response.ErrorMessage = "That trail doesn't connect to your current city!";
                        return response;
                    }
                }
            }

            long otherCityId = road.GetCityIdOnOtherEnd(position.CityId);

            coreData.Vars[TraderVars.RoadId] = road.IdKey;
            coreData.Vars[TraderVars.CityId] = otherCityId;
            coreData.Vars[TraderVars.DistanceAlongRoad] = 0;


            response.TargetCityId = coreData.Vars[TraderVars.CityId];
            response.RoadId = coreData.Vars[TraderVars.RoadId];
            response.DistanceTravelled = coreData.Vars[TraderVars.DistanceAlongRoad];

            return response;
        }

        public async Task<TurnAroundResponse> TurnAround(WebContext context, TurnAroundArgs args)
        {
            TurnAroundResponse response = new TurnAroundResponse()
            {

            };

            CoreData coreData = await context.GetAsync<CoreData>();

            CaravanPosition pos = _caravanService.GetPosition(coreData);

            Road road = _gameData.Get<RoadSettings>(context.core).Get(pos.RoadId);

            if (road == null)
            {
                response.ErrorMessage = "You aren't on a trail!";
                return response;
            }

            coreData.Vars[TraderVars.CityId] = road.GetCityIdOnOtherEnd(pos.TargetCityId);
            coreData.Vars[TraderVars.DistanceAlongRoad] = (long)(road.Distance - coreData.Vars[TraderVars.DistanceAlongRoad]);

            response.RoadId = road.IdKey;
            response.TargetCityId = coreData.Vars[TraderVars.CityId];
            response.DistanceTravelled = coreData.Vars[TraderVars.DistanceAlongRoad];
            response.Success = true;

            return response;
        }

    }
}
