using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.CurrencySpend.Entities;
using OxDb.SharedGame.Trader.CurrencySpend.Services;
using OxDb.SharedGame.Trader.CurrencySpend.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.CurrencySpend.SpendHelpers
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

        public abstract ValueTask<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity);

    }
}
