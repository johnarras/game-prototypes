using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Animals.Settings;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using System;
using System.Linq;

namespace Genrpg.Shared.Trader.Animals.Services
{

    public interface IAnimalService : IInjectable
    {
        void AddAnimalToHoldings(CoreData coreData, HoldingsData holdings, long animalTypeId);
        void AddSkinToHoldings(CoreData coreData, HoldingsData holdings, long skinTypeId);

        long GetAnimalQuantity(HoldingsData holdings, long animalTypeId);

        long GetSkinQuantity(HoldingsData holdings, long skinTypeId);
    }

    public class AnimalService : IAnimalService
    {

        private IGameData _gameData = null;

        public void AddAnimalToHoldings(CoreData coreData, HoldingsData holdings, long animalTypeId)
        {
            if (!holdings.AnimalsOwned.HasBitIndex(animalTypeId))
            {
                holdings.AnimalsOwned.SetBitIndex(animalTypeId);
            }

            SkinType skinType = _gameData.Get<SkinTypeSettings>(coreData).GetData().FirstOrDefault(x => x.AnimalTypeId == animalTypeId && x.IsDefault);
            if (skinType != null)
            {
                AddSkinToHoldings(coreData, holdings, skinType.IdKey);
            }
        }

        public void AddSkinToHoldings(CoreData coreData, HoldingsData holdings, long skinTypeId)
        {
            SkinType skinType = _gameData.Get<SkinTypeSettings>(coreData).Get(skinTypeId);

            if (skinType == null)
            {
                return;
            }

            holdings.SkinsOwned.SetBitIndex(skinTypeId);
        }

        public long GetAnimalQuantity(HoldingsData holdings, long animalTypeId)
        {
            return holdings.AnimalsOwned.HasBitIndex(animalTypeId) ? 1 : 0;
        }

        public long GetSkinQuantity(HoldingsData holdings, long skinTypeId)
        {
            return holdings.SkinsOwned.HasBitIndex(skinTypeId) ? 1 : 0; 
        }
    }
}
