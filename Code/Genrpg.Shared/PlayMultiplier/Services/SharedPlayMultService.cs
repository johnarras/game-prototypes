using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MobileGame.Constants;
using Genrpg.Shared.PlayMultiplier.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.PlayMultiplier.Services
{
    public class SharedPlayMultService : ISharedPlayMultService
    {
        private IGameData _gameData = null;
        public long GetMaxMult(CoreData coreData)
        {
            return GetValidMults(coreData).Last();
        }

        public List<long> GetValidMults(CoreData coreData)
        {
            PlayMultSettings settings = _gameData.Get<PlayMultSettings>(coreData);

            long totalEnergy = coreData.Currencies[CoreCurrencyTypes.Rations];

            long maxMult = (long)Math.Floor(settings.MaxMultAsPercentOfCurrentDice * totalEnergy);

            maxMult = MathUtils.Clamp(MobileGameConstants.MinPlayMult, maxMult, settings.MaxPlayMult);

            List<long> result = new List<long>();

            for (int i = 1; i <= maxMult; i++)
            {
                result.Add(i);
            }
            return result;
        }
    }
}


