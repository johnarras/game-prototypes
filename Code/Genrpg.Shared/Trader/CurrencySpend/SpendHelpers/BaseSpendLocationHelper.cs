using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Services;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.CurrencySpend.SpendHelpers
{
    public abstract class BaseSpendLocationHelper : ISpendLocationHelper
    {
        protected IGameData _gameData = null!;
        protected ICurrencySpendService _spendService = null!;
        protected ICaravanService _caravanService = null!;
        public abstract long HelperKey { get; }

        protected virtual SpendLocation GetSpendLocation(IFilteredObject obj)
        {
            return _gameData.Get<SpendLocationSettings>(obj).Get(HelperKey);
        }

        protected virtual SpendType GetSpendTypeWithReward(IFilteredObject obj, long entityTypeId, long entityId)
        {
            SpendLocation loc = GetSpendLocation(obj);

            if (loc == null)
            {
                return null;
            }

            List<SpendType> validSpends = new List<SpendType>();

            foreach (SpendType stype in loc.SpendTypes)
            {
                if (stype.Rewards.Any(x => x.EntityTypeId == entityTypeId && x.EntityId == entityId))
                {
                    validSpends.Add(stype);
                }
            }
            return validSpends.FirstOrDefault();
        }

        public abstract Task<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity);

    }
}
