using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Animals.Settings;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using System.Linq;

namespace Genrpg.Shared.Trader.Animals.Services
{

    public interface IAnimalService : IInjectable
    {
        void AddAnimalToHoldings(CoreData coreData, HoldingsData holdings, long animalTypeId);
        void AddSkinToHoldings(CoreData coreData, HoldingsData holdings, long skinTypeId);
    }

    public class AnimalService : IAnimalService
    {

        private IGameData _gameData = null;

        public void AddAnimalToHoldings(CoreData coreData, HoldingsData holdings, long animalTypeId)
        {
            if (!holdings.AnimalsOwned.HasBit(animalTypeId))
            {
                holdings.AnimalsOwned.SetBit(animalTypeId);
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

            holdings.SkinsOwned.SetBit(skinTypeId);
        }
    }
}
