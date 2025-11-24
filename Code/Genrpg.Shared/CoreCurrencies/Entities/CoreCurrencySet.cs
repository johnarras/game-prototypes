using Genrpg.Shared.CoreCurrencies.Constants;
using MessagePack;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.CoreCurrencies.Entities
{
    [MessagePackObject]
    public class CoreCurrencySet
    {
        [Key(0)] public List<CoreCurrencyStatus> _dat { get; set; } = new List<CoreCurrencyStatus>();

        public CoreCurrencyStatus GetStatus(long coreCurrencyTypeId)
        {
            CoreCurrencyStatus status = _dat.FirstOrDefault(x => x.CoreCurrencyTypeId == coreCurrencyTypeId);
            if (status == null)
            {
                status = new CoreCurrencyStatus()
                {
                    CoreCurrencyTypeId = coreCurrencyTypeId,
                };
                _dat.Add(status);
            }
            return status;
        }

        public void Set(long coreCurrencyTypeId, int index, long value)
        {
            GetStatus(coreCurrencyTypeId).Set(index, value);
        }
        public void Add(long coreCurrencyTypeId, int index, long value)
        {
            GetStatus(coreCurrencyTypeId).Add(index, value);
        }

        public long Curr(long coreCurrencyTypeId)
        {
            return GetStatus(coreCurrencyTypeId).Get(CurrencyDataOffset.Curr);
        }

        public long GetMaxStorage(long coreCurrencyTypeId)
        {
            return GetStatus(coreCurrencyTypeId).Storage();
        }

        public long GetTotalRegen(long coreCurrencyTypeId)
        {
            return GetStatus(coreCurrencyTypeId).Regen();
        }
    }
}
