using OxDb.RequestServer.Core;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Cities.Settings;
using OxDb.SharedGame.Trader.Constants;
using OxDb.SharedGame.Trader.Flags.Constants;
using OxDb.SharedGame.Trader.Holdings.PlayerData;
using OxDb.SharedGame.Trader.Maps.Services;
using OxDb.SharedGame.Trader.Travel.WebApi;

namespace OxDb.RequestServer.Trader.Travel.Services
{

    public interface IServerCaravanService : IInjectable
    {
        Task<HeadToTargetResponse> HeadToTarget(WebContext context, HeadToTargetRequest request, bool force);
        Task EnterCity(WebContext context, long cityId, bool force);
    }

    public class ServerCaravanService : IServerCaravanService
    {
        private IGameData _gameData = null;
        private ICaravanService _caravanService = null;
        private ITraderMapService _traderMapService = null;

        public async Task<HeadToTargetResponse> HeadToTarget(WebContext context, HeadToTargetRequest request, bool force)
        {
            HeadToTargetResponse response = new HeadToTargetResponse()
            {

            };

            CoreData coreData = await context.GetAsync<CoreData>();

            CaravanPosition position = _caravanService.GetPosition(coreData);

            if (request.ToX < 1 || request.ToY < 1)
            {
                response.ErrorMessage = "Need to set coordinates.";
            }

            long fromX = coreData.Vars[TraderVars.FromX];
            long fromY = coreData.Vars[TraderVars.FromY];

            bool onRoad = coreData.HasFlag(TraderFlags.OnRoad);

            coreData.Vars[TraderVars.ToX] = request.ToX;
            coreData.Vars[TraderVars.ToY] = request.ToY;
            coreData.Vars[TraderVars.FromX] = position.CurrX;
            coreData.Vars[TraderVars.FromY] = position.CurrY;
            coreData.Vars[TraderVars.DistanceGone] = 0;
            coreData.Vars[TraderVars.TotalDistanceToTarget] = await _traderMapService.GetDistanceBetweenPoints(
                context, position.CurrX, position.CurrY, request.ToX, request.ToY);
            coreData.Vars[TraderVars.CityId] = 0;
            City toCity = _gameData.Get<CitySettings>(coreData).GetData().FirstOrDefault(
                x => x.MapPixelX == request.ToX && x.MapPixelY == request.ToY);

            if (toCity != null)
            {
                coreData.Vars[TraderVars.CityId] = (int)toCity.IdKey;

                City currCity = _gameData.Get<CitySettings>(coreData).GetData().FirstOrDefault(x => x.MapPixelX == position.CurrX &&
                x.MapPixelY == position.CurrY);
                if (position.GetCurrentCity() != null || currCity != null ||
                    (fromX == toCity.MapPixelX && fromY == toCity.MapPixelY))
                {
                    coreData.AddFlag(TraderFlags.OnRoad);
                }
                else
                {
                    coreData.RemoveFlag(TraderFlags.OnRoad);
                }
            }
            else
            {
                coreData.RemoveFlag(TraderFlags.OnRoad);
            }

            response.FromX = coreData.Vars[TraderVars.FromX];
            response.FromY = coreData.Vars[TraderVars.FromY];
            response.ToX = coreData.Vars[TraderVars.ToX];
            response.ToY = coreData.Vars[TraderVars.ToY];
            response.TotalDistanceToTarget = coreData.Vars[TraderVars.TotalDistanceToTarget];
            response.ToCityId = coreData.Vars[TraderVars.CityId];
            response.NewTraderFlags = coreData.Vars[TraderVars.Flags];
            response.Success = true;
            return response;
        }


        public async Task EnterCity(WebContext context, long cityId, bool force)
        {
            CoreData coreData = await context.GetAsync<CoreData>();
            CaravanPosition position = _caravanService.GetPosition(coreData);

            HoldingsData holdings = await context.GetAsync<HoldingsData>();

            holdings.CitiesVisited.SetBitIndex(cityId);

            City city = _gameData.Get<CitySettings>(coreData).Get(cityId);

            if (city == null)
            {
                return;
            }

            if (!force)
            {
                if (position.GetCurrentCity() == null ||
                    position.GetCurrentCity().IdKey != cityId)
                {
                    return;
                }
            }

            coreData.Vars[TraderVars.FromX] = city.MapPixelX;
            coreData.Vars[TraderVars.FromY] = city.MapPixelY;
            coreData.Vars[TraderVars.ToX] = city.MapPixelX;
            coreData.Vars[TraderVars.ToY] = city.MapPixelY;
            coreData.Vars[TraderVars.DistanceGone] = 0;
            coreData.Vars[TraderVars.TotalDistanceToTarget] = 0;
            coreData.Vars[TraderVars.CityId] = (int)cityId;

        }
    }
}
