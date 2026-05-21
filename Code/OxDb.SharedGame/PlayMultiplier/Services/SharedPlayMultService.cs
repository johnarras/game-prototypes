using OxDb.SharedCore.GameSettings;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.PlayMultiplier.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.PlayMultiplier.Services
{
    public class SharedPlayMultService : ISharedPlayMultService
    {
        private IGameData _gameData = null;
        public int GetMaxMult(CoreData coreData)
        {
            return GetValidMults(coreData).Last();
        }

        public List<int> GetValidMults(CoreData coreData)
        {
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


