using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MobileGame.Constants;
using Genrpg.Shared.PlayMultiplier.Constants;
using Genrpg.Shared.PlayMultiplier.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.PlayMultiplier.Services
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


