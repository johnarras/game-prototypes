using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayMultiplier.Settings;
using Genrpg.Shared.Trader.Animals.Settings;
using Genrpg.Shared.Trader.Animals.WebApi;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using Genrpg.Shared.Trader.Stats.Constants;
using Genrpg.Shared.Trader.Stats.PlayerData;
using System;
using System.Linq;

namespace Genrpg.Shared.Trader.Caravans.Services
{
    public interface ICaravanService : IInjectable
    {
        void UpdateCoreStatsFromCaravan(CoreUserData userData, CaravanData caravanData, TraderStatData statData);

        CaravanTravelInfo GetTravelInfo(CoreUserData userData);

        AddAnimalToCaravanResult AddAnimalToCaravan(CoreUserData userData, CaravanData caravanData, HoldingsData holdings,
            TraderStatData statData,
            long animalTypeId, bool force);

        RemoveAnimalFromCaravanResult RemoveAnimalFromCaravan(CoreUserData userData,
            CaravanData caravanData, TraderStatData statsData, long animalTypeId);


    }

    public class CaravanService : ICaravanService
    {

        private IGameData _gameData = null;

        public void UpdateCoreStatsFromCaravan(CoreUserData userData, CaravanData caravanData, TraderStatData statData)
        {
            PlayMult mult = _gameData.Get<PlayMultSettings>(userData).Get(userData.Mult);

            if (mult == null)
            {
                mult = _gameData.Get<PlayMultSettings>(userData).GetData().First();
                userData.Mult = mult.Mult;
            }

            AnimalTypeSettings animalSettings = _gameData.Get<AnimalTypeSettings>(userData);

            long baseSpeed = 0;
            long currSpeed = 0;

            long suppliesCost = 0;

            int animalCount = 0;

            long totalCapacity = statData.Stats.Get(TraderStats.CaravanTradeGoods).Max();

            foreach (CaravanAnimal caravanAnimal in caravanData.Animals)
            {
                AnimalType animal = animalSettings.Get(caravanAnimal.AnimalTypeId);

                if (baseSpeed == 0 || animal.Speed < baseSpeed)
                {
                    baseSpeed = animal.Speed;
                }
                suppliesCost += animal.Supplies;
                totalCapacity += animal.Capacity;
                animalCount++;
            }

            currSpeed = baseSpeed;

            if (userData.FastMove)
            {
                currSpeed++;

                foreach (CaravanAnimal caravanAnimal in caravanData.Animals)
                {
                    AnimalType animal = animalSettings.Get(caravanAnimal.AnimalTypeId);

                    if (animal.Speed < currSpeed)
                    {
                        suppliesCost += animal.Cost;
                    }
                }
            }

            long overburden = Math.Max(0, caravanData.TradeGoods.Count - totalCapacity);

            if (overburden == 1)
            {
                suppliesCost *= 2;
            }
            else if (overburden > 0)
            {
                suppliesCost = 1000;
                baseSpeed = 0;
                currSpeed = 0;
            }

            // Now maybe modify stuff here based on user bonuses.


            userData.Dice = currSpeed;
            userData.Cost = suppliesCost;
            userData.Bonus = mult.BonusDistancePerDie * currSpeed;

        }
        public CaravanTravelInfo GetTravelInfo(CoreUserData userData)
        {
            CaravanTravelInfo info = new CaravanTravelInfo()
            {
                Days = userData.Mult,
                BonusDistancePerDay = userData.Bonus,
                CostPerDay = userData.Cost,
                DiceDistancePerDay = userData.Dice,
                TotalCost = userData.Cost * userData.Mult,
            };

            return info;
        }

        public AddAnimalToCaravanResult AddAnimalToCaravan(CoreUserData userData,
            CaravanData caravanData,
            HoldingsData holdings,
            TraderStatData statsData,
            long animalTypeId, bool force)
        {

            AddAnimalToCaravanResult result = new AddAnimalToCaravanResult()
            {
                Success = false,
                Travel = GetTravelInfo(userData),
            };

            CaravanPosition position = userData.GetPosition();

            if (position.CityId == 0 && !force)
            {
                result.ErrorMessage = "You can only swap animals in a city.";
                return result;
            }

            AnimalType animal = _gameData.Get<AnimalTypeSettings>(userData).Get(animalTypeId);

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

            if (statsData.Stats.Get(TraderStats.CaravanAnimals).Max() <= caravanData.Animals.Count)
            {
                result.ErrorMessage = "You can't add any more animals to your caravan.";
                return result;
            }

            caravanData.Animals.Add(new CaravanAnimal() { AnimalTypeId = animalTypeId, SkinTypeId = animalTypeId });

            result.Success = true;

            UpdateCoreStatsFromCaravan(userData, caravanData, statsData);

            result.Travel = GetTravelInfo(userData);

            return result;

        }

        public RemoveAnimalFromCaravanResult RemoveAnimalFromCaravan(CoreUserData userData,
            CaravanData caravanData, TraderStatData statsData, long animalId)
        {
            RemoveAnimalFromCaravanResult result = new RemoveAnimalFromCaravanResult()
            {
                Success = false,
                Travel = GetTravelInfo(userData),
            };

            CaravanPosition position = userData.GetPosition();

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

            UpdateCoreStatsFromCaravan(userData, caravanData, statsData);

            result.Travel = GetTravelInfo(userData);

            return result;

        }
    }
}


