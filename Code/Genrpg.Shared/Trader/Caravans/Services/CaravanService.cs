using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MobileGame.Constants;
using Genrpg.Shared.PlayMultiplier.Settings;
using Genrpg.Shared.Trader.Animals.Settings;
using Genrpg.Shared.Trader.Animals.WebApi;
using Genrpg.Shared.Trader.Buffs.Interfaces;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Flags.Constants;
using Genrpg.Shared.Trader.Flags.Settings;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using Genrpg.Shared.Trader.Maps.Services;
using Genrpg.Shared.Trader.Stats.Constants;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Travel.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System;
using System.Linq;

namespace Genrpg.Shared.Trader.Caravans.Services
{
    public interface ICaravanService : IInjectable
    {
        void UpdateTravelStats(CoreData coreData);

        void UpdateTravelStatsFromCaravan(CoreData coreData, CaravanData caravanData, TraderStatData statData);

        CaravanTravelInfo GetTravelInfo(CoreData coreData);

        AddAnimalToCaravanResult AddAnimalToCaravan(CoreData coreData, CaravanData caravanData, HoldingsData holdings,
            TraderStatData statData,
            long animalTypeId, bool force);

        RemoveAnimalFromCaravanResult RemoveAnimalFromCaravan(CoreData coreData,
            CaravanData caravanData, TraderStatData statsData, long animalTypeId);

        CaravanPosition GetPosition(CoreData coreData);
    }

    public class CaravanService : ICaravanService
    {

        private IGameData _gameData = null;
        private ITraderMapService _traderMapService = null;

        public void UpdateTravelStatsFromCaravan(CoreData coreData, CaravanData caravanData, TraderStatData statData)
        {
            AnimalTypeSettings animalSettings = _gameData.Get<AnimalTypeSettings>(coreData);



            long baseDiceSpeed = 0;
            long baseCapacity = statData.Stats[TraderStats.CaravanCapacity].Total();
            long baseRationsCost = statData.Stats[TraderStats.CaravanRationsCost].Total();
            long baseBonusSpeed = statData.Stats[TraderStats.CaravanBonusSpeed].Total();
            foreach (CaravanAnimal caravanAnimal in caravanData.Animals)
            {
                AnimalType animal = animalSettings.Get(caravanAnimal.AnimalTypeId);

                if (baseDiceSpeed < 1 || animal.Speed < baseDiceSpeed)
                {
                    baseDiceSpeed = animal.Speed;
                }

                baseCapacity += animal.Capacity;
                baseRationsCost += animal.Upkeep;
            }

            coreData.Vars[TraderVars.BaseCapacity] = baseCapacity;
            coreData.Vars[TraderVars.BaseDiceSpeed] = baseDiceSpeed;
            coreData.Vars[TraderVars.BaseRationsCost] = baseRationsCost;
            coreData.Vars[TraderVars.BaseBonusSpeed] = baseBonusSpeed;
            coreData.Vars[TraderVars.BaseForaging] = statData.Stats[TraderStats.Foraging].Total();
            coreData.Vars[TraderVars.BaseGoodLuckEvents] = statData.Stats[TraderStats.GoodLuckEvents].Total();
            coreData.Vars[TraderVars.BaseBadLuckEvents] = statData.Stats[TraderStats.BadLuckEvents].Total();
            UpdateTravelStats(coreData);
        }

        public virtual void UpdateTravelStats(CoreData coreData)
        {

            PlayMultSettings multSettings = _gameData.Get<PlayMultSettings>(coreData);

            coreData.Vars[TraderVars.Mult] = MathUtils.Clamp(MobileGameConstants.MinPlayMult, coreData.Vars[TraderVars.Mult], multSettings.MaxPlayMult);

            AnimalTypeSettings animalSettings = _gameData.Get<AnimalTypeSettings>(coreData);

            TravelSettings travelSettings = _gameData.Get<TravelSettings>(coreData);

            TraderFlagSettings flagSettings = _gameData.Get<TraderFlagSettings>(coreData);

            CaravanStatusBlock statusBlock = new CaravanStatusBlock();

            statusBlock.Capacity = coreData.Vars[TraderVars.BaseCapacity];
            statusBlock.RationsCost = coreData.Vars[TraderVars.BaseRationsCost];
            statusBlock.DiceSpeed = coreData.Vars[TraderVars.BaseDiceSpeed];
            statusBlock.BonusSpeed = coreData.Vars[TraderVars.BaseBonusSpeed];
            statusBlock.ItemCount = coreData.Vars[TraderVars.ItemCount];
            statusBlock.ForageChance = travelSettings.BaseForageChance + 0.01 * coreData.Vars[TraderVars.ForageChance];
            statusBlock.BadEventChance = travelSettings.BaseBadEventChance + 0.01 * coreData.Vars[TraderVars.BaseBadLuckEvents];
            statusBlock.GoodEventChance = travelSettings.BaseGoodEventChance + 0.01 * coreData.Vars[TraderVars.BaseGoodLuckEvents];

            foreach (TraderFlag flag in flagSettings.GetData())
            {
                if (coreData.HasFlag(flag.IdKey))
                {
                    UpdateStatusBlockFromBuff(statusBlock, flag);
                }
            }

            long totalCost = statusBlock.DiceSpeed * coreData.Vars[TraderVars.Mult];

            statusBlock.BonusSpeed += (long)Math.Ceiling(multSettings.ExtraDailyDistPerTotalDice * totalCost);

            coreData.Vars[TraderVars.DiceSpeed] = statusBlock.DiceSpeed;
            coreData.Vars[TraderVars.RationsCost] = statusBlock.RationsCost;
            coreData.Vars[TraderVars.BonusSpeed] = statusBlock.BonusSpeed;
            coreData.Vars[TraderVars.ForageChance] = (long)(100 * statusBlock.ForageChance);
            coreData.Vars[TraderVars.BadEventChance] = (long)(100 * statusBlock.BadEventChance);
            coreData.Vars[TraderVars.GoodEventChance] = (long)(100 * statusBlock.GoodEventChance);
            coreData.Vars[TraderVars.MaxCapacity] = statusBlock.Capacity;

            bool changedSomething = false;
            if (coreData.Vars[TraderVars.ItemCount] > coreData.Vars[TraderVars.MaxCapacity] &&
                !coreData.HasFlag(TraderFlags.Overloaded))
            {
                coreData.AddFlag(TraderFlags.Overloaded);
                UpdateTravelStats(coreData);
            }
        }

        private void UpdateStatusBlockFromBuff(CaravanStatusBlock statusBlock, ITravelBuff buff)
        {

            statusBlock.BonusSpeed += buff.BonusSpeed * statusBlock.DiceSpeed;
            statusBlock.RationsCost += buff.RationsCost;
            statusBlock.Capacity += buff.Capacity;
            statusBlock.ForageChance += buff.ForageChance;
            statusBlock.BadEventChance += buff.BadEventChance;
            statusBlock.GoodEventChance += buff.GoodEventChance;
        }


        public CaravanTravelInfo GetTravelInfo(CoreData coreData)
        {
            long rationsCost = Math.Max(1, coreData.Vars[TraderVars.RationsCost]);
            CaravanTravelInfo info = new CaravanTravelInfo()
            {
                Days = coreData.Vars[TraderVars.Mult],
                CostPerDay = rationsCost,
                TotalCost = rationsCost * coreData.Vars[TraderVars.Mult],
                DiceSpeed = coreData.Vars[TraderVars.DiceSpeed],
                BonusSpeed = coreData.Vars[TraderVars.BonusSpeed],
            };

            return info;
        }

        public AddAnimalToCaravanResult AddAnimalToCaravan(CoreData coreData,
            CaravanData caravanData,
            HoldingsData holdings,
            TraderStatData statsData,
            long animalTypeId, bool force)
        {

            AddAnimalToCaravanResult result = new AddAnimalToCaravanResult()
            {
                Success = false,
                Travel = GetTravelInfo(coreData),
            };

            CaravanPosition position = GetPosition(coreData);

            if (position.GetCurrentCity() == null && !force)
            {
                result.ErrorMessage = "You can only swap animals in a city.";
                return result;
            }

            AnimalType animal = _gameData.Get<AnimalTypeSettings>(coreData).Get(animalTypeId);

            if (animal == null)
            {
                result.ErrorMessage = "That animal doesn't exist.";
                return result;
            }

            if (!holdings.AnimalsOwned.HasBit(animalTypeId))
            {
                result.ErrorMessage = "You don't own that animal.";
                return result;
            }

            if (caravanData.Animals.Any(x => x.AnimalTypeId == animalTypeId))
            {
                result.ErrorMessage = "This animal is already in your caravan.";
                return result;
            }

            if (statsData.Stats[TraderStats.CaravanAnimals].Total() <= caravanData.Animals.Count)
            {
                result.ErrorMessage = "You can't add any more animals to your caravan.";
                return result;
            }

            caravanData.Animals.Add(new CaravanAnimal() { AnimalTypeId = animalTypeId, SkinTypeId = animalTypeId });

            result.Success = true;

            UpdateTravelStatsFromCaravan(coreData, caravanData, statsData);

            result.Travel = GetTravelInfo(coreData);

            return result;

        }

        public RemoveAnimalFromCaravanResult RemoveAnimalFromCaravan(CoreData coreData,
            CaravanData caravanData, TraderStatData statsData, long animalId)
        {
            RemoveAnimalFromCaravanResult result = new RemoveAnimalFromCaravanResult()
            {
                Success = false,
                Travel = GetTravelInfo(coreData),
            };

            CaravanPosition position = GetPosition(coreData);

            if (position.GetCurrentCity() == null)
            {
                result.ErrorMessage = "You can only swap animals in a city.";
                return result;
            }

            CaravanAnimal caravanAnimal = caravanData.Animals.FirstOrDefault(x => x.AnimalTypeId == animalId);

            if (caravanAnimal == null)
            {
                result.ErrorMessage = "This animal is not in your caravan.";
                return result;
            }

            caravanData.Animals.Remove(caravanAnimal);

            result.Success = true;

            UpdateTravelStatsFromCaravan(coreData, caravanData, statsData);

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

            pos.DistanceToTarget = coreData.Vars[TraderVars.DistanceToTarget];
            pos.DistanceGone = coreData.Vars[TraderVars.DistanceGone];

            pos.Angle = _traderMapService.GetAngle(pos.FromX, pos.FromY, pos.ToX, pos.ToY);

            double percentGone = 0;

            if (pos.DistanceToTarget > 0)
            {
                percentGone = 1.0f * pos.DistanceGone / pos.DistanceToTarget;
            }

            MyPointF currPos = _traderMapService.GetMapCoordinate(pos.FromX, pos.FromY, pos.ToX, pos.ToY, pos.DistanceGone, pos.DistanceToTarget);

            pos.CurrX = (long)currPos.X;
            pos.CurrY = (long)currPos.Y;


            return pos;
        }

    }
}


