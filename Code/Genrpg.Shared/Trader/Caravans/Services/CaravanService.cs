using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayMultiplier.Settings;
using Genrpg.Shared.Trader.Animals.Settings;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using System.Linq;

namespace Genrpg.Shared.Trader.Caravans.Services
{
    public interface ICaravanService : IInjectable
    {
        void UpdateCoreStatsFromCaravan(CoreUserData userData, CaravanData caravanData);

        TravelParams GetTravelParams(CoreUserData userData);
    }

    public class CaravanService : ICaravanService
    {

        private IGameData _gameData = null;

        public void UpdateCoreStatsFromCaravan(CoreUserData userData, CaravanData caravanData)
        {
            PlayMult mult = _gameData.Get<PlayMultSettings>(userData).Get(userData.Mult);

            if (mult == null)
            {
                mult = _gameData.Get<PlayMultSettings>(userData).GetData().First();
                userData.Mult = mult.Mult;
            }

            AnimalSettings animalSettings = _gameData.Get<AnimalSettings>(userData);

            long baseSpeed = 0;
            long currSpeed = 0;

            long suppliesCost = 0;

            foreach (CaravanAnimal caravanAnimal in caravanData.Animals)
            {
                Animal animal = animalSettings.Get(caravanAnimal.AnimalTypeId);

                if (baseSpeed == 0 || animal.Speed < baseSpeed)
                {
                    baseSpeed = animal.Speed;
                }
                suppliesCost += animal.Supplies;
            }

            currSpeed = baseSpeed;

            if (userData.FastMove)
            {
                currSpeed++;

                foreach (CaravanAnimal caravanAnimal in caravanData.Animals)
                {
                    Animal animal = animalSettings.Get(caravanAnimal.AnimalTypeId);

                    if (animal.Speed < currSpeed)
                    {
                        suppliesCost += animal.Cost;
                    }
                }
            }

            // Now maybe modify stuff here based on user bonuses.


            userData.Dice = currSpeed;
            userData.Cost = suppliesCost;
            userData.Bonus = mult.BonusDistancePerSpeed * currSpeed;


        }
        public TravelParams GetTravelParams(CoreUserData userData)
        {
            TravelParams tp = new TravelParams()
            {
                Days = userData.Mult,
                BonusPerDay = userData.Bonus,
                CostPerDay = userData.Cost,
                DicePerDay = userData.Dice,
                TotalCost = userData.Cost * userData.Mult,
            };

            return tp;
        }
    }
}
