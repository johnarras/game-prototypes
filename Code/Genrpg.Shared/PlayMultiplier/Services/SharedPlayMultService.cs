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
        public int GetMaxMult(CoreData coreData)
        {
            return GetValidMults(coreData).Last();
        }

        public List<int> GetValidMults(CoreData coreData)
        {
            PlayMultSettings settings = _gameData.Get<PlayMultSettings>(coreData);

            long totalEnergy = coreData.Currencies[CoreCurrencyTypes.Rations];

            int maxMult = (int)Math.Floor(settings.MaxMultAsPercentOfCurrentDice * totalEnergy);

            maxMult = MathUtil.Clamp(MobileGameConstants.MinPlayMult, maxMult, settings.MaxPlayMult);

            List<int> result = new List<int>();

            for (int i = 1; i <= maxMult; i++)
            {
                result.Add(i);
            }
            return result;
        }
    }
}


