using Genrpg.Shared.Attributes.Constants;
using Genrpg.Shared.Attributes.PlayerData;
using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.Attributes.Settings;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.CaravanMembers.Settings;
using Genrpg.Shared.Trader.CaravanMembers.WebApi;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using Genrpg.Shared.Trader.Maps.Services;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.Caravans.Services
{
    public interface ICaravanService : IInitializable
    {
        Task CalcCoreTravelStats(IUnitDataLookup lookup);

        CaravanTravelInfo GetTravelInfo(CoreData coreData);

        Task<AddMemberToCaravanResult> AddMemberToCaravan(IUnitDataLookup lookup, long CaravanMemberId, bool force);

        Task<RemoveMemberFromCaravanResult> RemoveMemberFromCaravan(IUnitDataLookup lookup, long caravanMemberId, bool force);

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


        private List<GameplayStatToTraderVarMapper> _statMappers = new List<GameplayStatToTraderVarMapper>();
        public virtual async Task Initialize(CancellationToken token)
        {

            // Explicit list rather than reflection since this gets done at runtime.
            _statMappers.Add(new GameplayStatToTraderVarMapper(GameplayStats.MaxSize,TraderVars.MaxSize));
            _statMappers.Add(new GameplayStatToTraderVarMapper(GameplayStats.MaxInventory, TraderVars.MaxInventory));
            _statMappers.Add(new GameplayStatToTraderVarMapper(GameplayStats.Luck, TraderVars.Luck));
            _statMappers.Add(new GameplayStatToTraderVarMapper(GameplayStats.Searching, TraderVars.Searching));
            _statMappers.Add(new GameplayStatToTraderVarMapper(GameplayStats.BonusSpeed, TraderVars.BonusSpeedPerDie));

            await Task.CompletedTask;
        }


        protected async Task UpdateBuffsFromBitList<TParent, TChild, TEffect>(IUnitDataLookup lookup, CoreData coreData, long memberBits)
            where TParent : ParentSettings<TChild>
            where TChild : ChildSettings, IId, IEffectList<TEffect>, new()
            where TEffect : class, IEffect
        {

            IReadOnlyList<TChild> children = _gameData.Get<TParent>(coreData).GetData();

            foreach (TChild child in children)
            {
                if (FlagUtils.HasBitIndex(memberBits, child.IdKey))
                {
                    foreach (TEffect effect in child.Effects)
                    {
                        await _attributeService.ApplyBuffEffect(lookup, effect);
                    }
                }
            }
        }


        public virtual async Task CalcCoreTravelStats(IUnitDataLookup lookup)
        {

            CoreData coreData = await lookup.GetAsync<CoreData>();
            CaravanMemberSettings memberSettings = _gameData.Get<CaravanMemberSettings>(coreData);
            AttributeData attributeData = await lookup.GetAsync<AttributeData>();
            CaravanData caravanData = await lookup.GetAsync<CaravanData>();

            attributeData.ResetBuffs();

            await UpdateBuffsFromBitList<GameplayBuffSettings, GameplayBuff, Effect>(lookup, coreData, coreData.Vars[TraderVars.BuffBits]);
            await UpdateBuffsFromBitList<GameplayDebuffSettings, GameplayDebuff, Effect>(lookup, coreData, coreData.Vars[TraderVars.DebuffBits]);


            int baseDiceSpeed = 0;
            bool didSetDiceSpeed = false;
            foreach (CurrentCaravanMember currentMember in caravanData.CurrentMembers)
            {
                CaravanMember caravanMember = memberSettings.Get(currentMember.CaravanMemberId);

                if (caravanMember != null)
                {
                    foreach (Effect effect in caravanMember.Effects)
                    {
                        await _attributeService.ApplyBuffEffect(lookup, effect);
                    }

                    if (caravanMember.Speed == 0)
                    {
                        continue;
                    }

                    if (baseDiceSpeed == 0)
                    {
                        baseDiceSpeed = caravanMember.Speed;
                    }
                    else if (caravanMember.Speed < baseDiceSpeed)
                    {
                        baseDiceSpeed = caravanMember.Speed;
                    }
                    didSetDiceSpeed = true;
                }
            }

            if (!didSetDiceSpeed)
            {
                baseDiceSpeed = 0;
            }

            coreData.Vars[TraderVars.BaseDiceSpeed] = baseDiceSpeed;

            foreach (GameplayStatToTraderVarMapper mapper in _statMappers)
            {
                coreData.Vars[mapper.ToTraderVarId] = (int)attributeData.GetQuantity(EAttributeCategories.Stats, EAttributeValIndex.Total, mapper.FromGameplayStatId);
            }

            coreData.TravelDayCurrencies.Clear();

            for (int c = 0; c < attributeData.TravelDayCurrencies.Count(); c++)
            {
                long total = attributeData.TravelDayCurrencies[c].Total();

                if (total != 0)
                {
                    coreData.TravelDayCurrencies[c] = (int)total;
                }
            }

            int sizeUsed = 0;
            foreach (CurrentCaravanMember currentMember in caravanData.CurrentMembers)
            {
                CaravanMember member = memberSettings.Get(currentMember.CaravanMemberId);

                sizeUsed += member.Size;
            }

            coreData.Vars[TraderVars.SizeUsed] = sizeUsed;
            coreData.Vars[TraderVars.InventoryUsed] = caravanData.TradeGoods.Count;
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

        public async Task<AddMemberToCaravanResult> AddMemberToCaravan(IUnitDataLookup lookup, long CaravanMemberId, bool force)
        {

            CoreData coreData = await lookup.GetAsync<CoreData>();
            AddMemberToCaravanResult result = new AddMemberToCaravanResult()
            {
                Success = false,
                Travel = GetTravelInfo(coreData),
            };

            CaravanPosition position = GetPosition(coreData);

            if (position.GetCurrentCity() == null && !force)
            {
                result.ErrorMessage = "You can only swap Caravan Members in a city.";
                return result;
            }

            CaravanMember CaravanMember = _gameData.Get<CaravanMemberSettings>(coreData).Get(CaravanMemberId);

            if (CaravanMember == null)
            {
                result.ErrorMessage = "That Caravan Member doesn't exist.";
                return result;
            }

            HoldingsData holdings = await lookup.GetAsync<HoldingsData>();

            if (!holdings.CaravanMembersOwned.HasBitIndex(CaravanMemberId))
            {
                result.ErrorMessage = "You don't own that Caravan Member.";
                return result;
            }

            CaravanData caravanData = await lookup.GetAsync<CaravanData>();

            if (caravanData.CurrentMembers.Any(x => x.CaravanMemberId == CaravanMemberId))
            {
                result.ErrorMessage = "This Caravan Member is already in your caravan.";
                return result;
            }

            AttributeData attributeData = await lookup.GetAsync<AttributeData>();

            caravanData.CurrentMembers.Clear();

            caravanData.CurrentMembers.Add(new CurrentCaravanMember() { CaravanMemberId = CaravanMemberId, SkinTypeId = CaravanMemberId });

            result.Success = true;

            await CalcCoreTravelStats(lookup);

            result.Travel = GetTravelInfo(coreData);

            return result;

        }

        public async Task<RemoveMemberFromCaravanResult> RemoveMemberFromCaravan(IUnitDataLookup lookup, long CaravanMemberId, bool force)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();
            RemoveMemberFromCaravanResult result = new RemoveMemberFromCaravanResult()
            {
                Success = false,
                Travel = GetTravelInfo(coreData),
            };

            CaravanPosition position = GetPosition(coreData);

            if (position.GetCurrentCity() == null && !force)
            {
                result.ErrorMessage = "You can only swap Caravan Members in a city.";
                return result;
            }

            CaravanData caravanData = await lookup.GetAsync<CaravanData>();

            CurrentCaravanMember caravanCaravanMember = caravanData.CurrentMembers.FirstOrDefault(x => x.CaravanMemberId == CaravanMemberId);

            if (caravanCaravanMember == null)
            {
                result.ErrorMessage = "This Caravan Member is not in your caravan.";
                return result;
            }

            caravanData.CurrentMembers.Remove(caravanCaravanMember);

            result.Success = true;

            await CalcCoreTravelStats(lookup);

            result.Travel = GetTravelInfo(coreData);

            return result;
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


            return pos;
        }
    }
}


