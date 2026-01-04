using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MobileGame.Constants;
using Genrpg.Shared.PlayMultiplier.Settings;
using Genrpg.Shared.Trader.Animals.Settings;
using Genrpg.Shared.Trader.Animals.WebApi;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using Genrpg.Shared.Trader.Roads.Settings;
using Genrpg.Shared.Trader.Stats.Constants;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Utils;
using System;
using System.Linq;

namespace Genrpg.Shared.Trader.Caravans.Services
{
    public interface ICaravanService : IInjectable
    {
        void UpdateCoreStatsFromCaravan(CoreData coreData, CaravanData caravanData, TraderStatData statData);

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

        public void UpdateCoreStatsFromCaravan(CoreData coreData, CaravanData caravanData, TraderStatData statData)
        {

            PlayMultSettings multSettings = _gameData.Get<PlayMultSettings>(coreData);

            coreData.Vars[TraderVars.Mult] = MathUtils.Clamp(MobileGameConstants.MinPlayMult, coreData.Vars[TraderVars.Mult], multSettings.MaxPlayMult);

            AnimalTypeSettings animalSettings = _gameData.Get<AnimalTypeSettings>(coreData);

            long baseSpeed = 0;
            long currSpeed = 0;

            long rationsCost = 0;

            int animalCount = 0;

            long totalCapacity = statData.Stats[TraderStats.CaravanTradeGoods].Max();

            foreach (CaravanAnimal caravanAnimal in caravanData.Animals)
            {
                AnimalType animal = animalSettings.Get(caravanAnimal.AnimalTypeId);

                if (baseSpeed == 0 || animal.Speed < baseSpeed)
                {
                    baseSpeed = animal.Speed;
                }
                rationsCost += animal.Upkeep;
                totalCapacity += animal.Capacity;
                animalCount++;
            }

            currSpeed = baseSpeed;

            if (coreData.Flags.HasBit(TraderFlags.FastMove))
            {
                currSpeed++;

                foreach (CaravanAnimal caravanAnimal in caravanData.Animals)
                {
                    AnimalType animal = animalSettings.Get(caravanAnimal.AnimalTypeId);

                    if (animal.Speed < currSpeed)
                    {
                        rationsCost += animal.Price;
                    }
                }
            }

            long overburden = Math.Max(0, caravanData.TradeGoods.Count - totalCapacity);

            if (overburden == 1)
            {
                rationsCost *= 2;
            }
            else if (overburden > 0)
            {
                rationsCost = 1000;
                baseSpeed = 0;
                currSpeed = 0;
            }

            // Now maybe modify stuff here based on user bonuses.


            long totalCost = currSpeed * coreData.Vars[TraderVars.Mult];

            double bonusDice = Math.Ceiling(multSettings.ExtraDailyDistPerTotalDice * totalCost);

            coreData.Vars[TraderVars.DiceSpeed] = currSpeed;
            coreData.Vars[TraderVars.RationsCost] = rationsCost;
            coreData.Vars[TraderVars.BonusSpeed] = (long)bonusDice;

        }
        public CaravanTravelInfo GetTravelInfo(CoreData coreData)
        {
            CaravanTravelInfo info = new CaravanTravelInfo()
            {
                Days = coreData.Vars[TraderVars.Mult],
                BonusDistancePerDay = coreData.Vars[TraderVars.BonusSpeed],
                CostPerDay = coreData.Vars[TraderVars.RationsCost],
                DiceDistancePerDay = coreData.Vars[TraderVars.DiceSpeed],
                TotalCost = coreData.Vars[TraderVars.RationsCost] * coreData.Vars[TraderVars.Mult]
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

            if (position.CityId == 0 && !force)
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

            if (statsData.Stats[TraderStats.CaravanAnimals].Max() <= caravanData.Animals.Count)
            {
                result.ErrorMessage = "You can't add any more animals to your caravan.";
                return result;
            }

            caravanData.Animals.Add(new CaravanAnimal() { AnimalTypeId = animalTypeId, SkinTypeId = animalTypeId });

            result.Success = true;

            UpdateCoreStatsFromCaravan(coreData, caravanData, statsData);

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

            if (position.CityId == 0)
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

            UpdateCoreStatsFromCaravan(coreData, caravanData, statsData);

            result.Travel = GetTravelInfo(coreData);

            return result;

        }

        public CaravanPosition GetPosition(CoreData coreData)
        {
            CaravanPosition pos = new CaravanPosition();

            Road road = _gameData.Get<RoadSettings>(coreData).Get(coreData.Vars[TraderVars.RoadId]);

            if (road != null)
            {
                pos.RoadId = road.IdKey;

                pos.TargetCityId = coreData.Vars[TraderVars.CityId];

                pos.DistanceTravelled = coreData.Vars[TraderVars.DistanceAlongRoad];

                if (coreData.Vars[TraderVars.DistanceAlongRoad] >= road.Distance)
                {
                    pos.OutsideOfCityId = pos.TargetCityId;
                }
                else if (coreData.Vars[TraderVars.DistanceAlongRoad] == 0)
                {
                    pos.OutsideOfCityId = road.GetCityIdOnOtherEnd(pos.TargetCityId);
                }
            }
            else if (coreData.Vars[TraderVars.CityId] > 0)
            {
                pos.CityId = coreData.Vars[TraderVars.CityId];
            }

            return pos;
        }

    }
}


