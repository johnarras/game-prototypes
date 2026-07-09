using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.PlayMultiplier.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OxDb.SharedGame.PlayMultiplier.Services
{
    public interface ISharedPlayMultService : IInjectable
    {
        ValueTask<int> GetMaxMult(IUnitDataLookup lookup);

        ValueTask<List<int>> GetValidMults(IUnitDataLookup lookup);
    }
    public class SharedPlayMultService : ISharedPlayMultService
    {
        private IGameData _gameData = null;
        public async ValueTask<int> GetMaxMult(IUnitDataLookup lookup)
        {
            return (await GetValidMults(lookup)).Last();
        }

        public async ValueTask<List<int>> GetValidMults(IUnitDataLookup lookup)
        {

            CoreData coreData = await lookup.GetAsync<CoreData>();
            PlayMultSettings settings = _gameData.Get<PlayMultSettings>(coreData);

            long maxMult = settings.MaxPlayMult;

            for (int i = 0; i < coreData.TravelDayCurrencies.Count(); i++)
            {
                if (coreData.TravelDayCurrencies[i] < 0)
                {
                    long dailyValue = -coreData.TravelDayCurrencies[i];

                    long currentValueAvailable = (long)(coreData.Currencies[i] * settings.MaxMultAsAPercentOfCurrentCurrency);

                    long currMaxMult = currentValueAvailable / dailyValue;

                    if (currMaxMult < maxMult)
                    {
                        maxMult = Math.Max(1, currMaxMult);
                    }
                }
            }

            List<int> result = new List<int>();

            for (int i = 1; i <= maxMult; i++)
            {
                result.Add(i);
            }
            return result;
        }
    }
}


