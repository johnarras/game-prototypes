using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Attributes.PlayerData;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.CaravanMembers.Settings;
using OxDb.SharedGame.Trader.CaravanMembers.WebApi;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.PlayerData;
using OxDb.SharedGame.Trader.Cities.Settings;
using OxDb.SharedGame.Trader.Constants;
using OxDb.SharedGame.Trader.Holdings.PlayerData;
using OxDb.SharedGame.Trader.Maps.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.Caravans.Services
{
    public interface ICaravanService : IInitializable
    {
        CaravanTravelInfo GetTravelInfo(CoreData coreData);

        Task<UpdateCaravanMembersResponse> UpdateCaravanMembers(IUnitDataLookup lookup, List<long> caravanMemberIds);

        CaravanPosition GetPosition(CoreData coreData);
    }


    class GameplayStatToTraderVarMapper
    {
        public long ToTraderVarId { get; set; }
        public long FromGameplayStatId { get; set; }

        public GameplayStatToTraderVarMapper(long fromGameplayStatId, long toTraderVarId)
        {
            ToTraderVarId = toTraderVarId;
            FromGameplayStatId = fromGameplayStatId;
        }
    }

    public class CaravanService : ICaravanService
    {

        private IGameData _gameData = null;
        private ITraderMapService _traderMapService = null;
        private IAttributeService _attributeService = null;
        protected ICalcAttributeService _calcAttributeService = null;


        public virtual async Task Initialize(CancellationToken token)
        {


            await Task.CompletedTask;
        }

        public CaravanTravelInfo GetTravelInfo(CoreData coreData)
        {
            SmallIdLongCollection dailyCurrencies = coreData.TravelDayCurrencies;

            long rationsCost = 0;// Math.Max(1, coreData.Vars[TraderVars.RationsCost]);
            CaravanTravelInfo info = new CaravanTravelInfo()
            {
                Days = coreData.Vars[TraderVars.Mult],
                DiceSpeed = coreData.GetDiceSpeed(),
                BonusSpeed = coreData.GetBonusSpeed(),
                MaxInventory = coreData.Vars[TraderVars.MaxInventory],
                InventoryUsed = coreData.Vars[TraderVars.InventoryUsed],
                MaxSize = coreData.Vars[TraderVars.MaxSize],
                SizeUsed = coreData.Vars[TraderVars.SizeUsed],
            };

            info.CurrenciesPerDay.CopyFrom(dailyCurrencies);

            return info;
        }

        public async Task<UpdateCaravanMembersResponse> UpdateCaravanMembers(IUnitDataLookup lookup, List<long> caravanMemberIds)
        {

            CoreData coreData = await lookup.GetAsync<CoreData>();
            UpdateCaravanMembersResponse response = new UpdateCaravanMembersResponse()
            {
                Success = false,
            };

            CaravanPosition position = GetPosition(coreData);


            HoldingsData holdings = await lookup.GetAsync<HoldingsData>();

            foreach (long memberId in caravanMemberIds)
            {
                CaravanMember caravanMember = _gameData.Get<CaravanMemberSettings>(coreData).Get(memberId);

                if (caravanMember == null)
                {
                    response.ErrorMessage = $"Caravan member {memberId} doesn't exist.";
                    return response;
                }

                if (!holdings.CaravanMembersOwned.HasBitIndex(memberId))
                {
                    response.ErrorMessage = "You don't have access to " + caravanMember.Name;
                    return response;
                }

            }


            if (caravanMemberIds.Distinct().Count() != caravanMemberIds.Count())
            {
                response.ErrorMessage = "You may only have one of each member in your caravan.";
                return response;
            }

            CaravanData caravanData = await lookup.GetAsync<CaravanData>();

            AttributesData attributeData = await lookup.GetAsync<AttributesData>();

            caravanData.CurrentMembers.Clear();

            foreach (long caravanMemberId in caravanMemberIds)
            {
                caravanData.CurrentMembers.Add(new CurrentCaravanMember() { CaravanMemberId = caravanMemberId });
            }

            response.Success = true;

            await _calcAttributeService.CalcBuffs(lookup);

            response.CurrentMembers = new List<CurrentCaravanMember>(caravanData.CurrentMembers);

            return response;
        }

        public CaravanPosition GetPosition(CoreData coreData)
        {
            CaravanPosition pos = new CaravanPosition();

            pos.FromX = coreData.Vars[TraderVars.FromX];
            pos.FromY = coreData.Vars[TraderVars.FromY];
            pos.ToX = coreData.Vars[TraderVars.ToX];
            pos.ToY = coreData.Vars[TraderVars.ToY];
            pos.TargetCity = _gameData.Get<CitySettings>(coreData).Get(coreData.Vars[TraderVars.CityId]);

            pos.TotalDistanceToTarget = coreData.Vars[TraderVars.TotalDistanceToTarget];
            pos.DistanceGone = coreData.Vars[TraderVars.DistanceGone];


            pos.Angle = _traderMapService.GetAngle(pos.FromX, pos.FromY, pos.ToX, pos.ToY);

            double percentGone = 0;

            if (pos.TotalDistanceToTarget > 0)
            {
                percentGone = 1.0f * pos.DistanceGone / pos.TotalDistanceToTarget;
            }

            MyPointF currPos = _traderMapService.GetMapCoordinate(pos.FromX, pos.FromY, pos.ToX, pos.ToY, pos.DistanceGone, pos.TotalDistanceToTarget);

            pos.CurrX = (int)currPos.X;
            pos.CurrY = (int)currPos.Y;

            if (pos.DistanceGone == 0)
            {
                pos.PositionCity = _gameData.Get<CitySettings>(coreData).GetData()
                    .FirstOrDefault(x => x.MapPixelX == pos.CurrX && x.MapPixelY == pos.CurrY);
            }

            return pos;
        }
    }
}


