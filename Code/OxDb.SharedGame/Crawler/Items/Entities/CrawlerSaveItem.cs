using OxDb.SharedCore.Effects.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OxDb.SharedGame.Crawler.Items.Entities
{
    public class CIdx
    {
        public const long ItemTypeId = 0;
        public const long LootRankId = 1;
        public const long Level = 2;
        public const long EquipSlotId = 4;
        public const long BuyCost = 5;
        public const long SellValue = 6;
        public const long Max = 8;

    }

    public class SaveEffect
    {
        public long[] Dat { get; set; } = new long[3];

        public SaveEffect()
        {
            Dat = new long[3];
        }

        public SaveEffect(IEffect eff)
        {
            Dat = new long[3] { eff.EntityTypeId, eff.EntityId, eff.Quantity };
        }
    }

    public class CrawlerSaveItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Dat { get; set; }

        private long[] _dat = null;

        public List<SaveEffect> SaveEffects { get; set; } = new List<SaveEffect>();


        public long Get(long index)
        {
            InternalSetupDat();

            return _dat[index];
        }
        public void Set(long index, long value)
        {
            InternalSetupDat();
            _dat[index] = value;
        }

        private void InternalSetupDat()
        {
            if (_dat == null)
            {
                _dat = new long[CIdx.Max];
            }

            if (string.IsNullOrEmpty(Dat))
            {
                return;
            }

            string[] words = Dat.Split(' ');

            for (int i = 0; i < words.Length && i < CIdx.Max; i++)
            {
                if (Int64.TryParse(words[i], out long val))
                {
                    _dat[i] = val;
                }
            }
        }

        public void CreateDatString()
        {
            InternalSetupDat();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _dat.Length; i++)
            {
                sb.Append(_dat[i] + " ");
            }

            Dat = sb.ToString();

        }
    }


}


