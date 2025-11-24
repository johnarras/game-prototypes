using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.Utils.Data;
using MessagePack;
using System;

namespace Genrpg.Shared.CoreCurrencies.Entities
{

    [MessagePackObject]
    public class CoreCurrencyStatus
    {
        [Key(0)] public long CoreCurrencyTypeId { get; set; }
        [Key(1)] public SmallIdLongCollection _dat { get; set; } = new SmallIdLongCollection();
        [Key(2)] public DateTime NextRegenTick { get; set; }


        public void CopyDataFrom(CoreCurrencyStatus other)
        {
            _dat = other._dat;
            NextRegenTick = other.NextRegenTick;
            CoreCurrencyTypeId = other.CoreCurrencyTypeId;
        }

        public void Set(long index, long quantity)
        {
            _dat.Set(index, Math.Max(quantity, 0));
            if (_dat.Get(CurrencyDataOffset.Curr) < _dat.Get(Storage()) && NextRegenTick < DateTime.UtcNow)
            {
                NextRegenTick = DateTime.UtcNow.AddHours(1);
            }
        }

        public void Add(long index, long quantity)
        {
            _dat.Add(index, quantity);
        }

        public long Get(long index)
        {
            return _dat.Get(index);
        }

        public void AddCurr(long quantity)
        {
            Add(CurrencyDataOffset.Curr, quantity);
        }

        public long Curr() { return Get(CurrencyDataOffset.Curr); }
        public long Regen() { return Get(CurrencyDataOffset.BaseRegen) + Get(CurrencyDataOffset.BonusRegen); }
        public long Storage() { return Get(CurrencyDataOffset.BaseStorage) + Get(CurrencyDataOffset.BonusStorage); }
    }
}
