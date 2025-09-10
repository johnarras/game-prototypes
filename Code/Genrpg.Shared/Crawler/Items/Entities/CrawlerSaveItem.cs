using Genrpg.Shared.Inventory.PlayerData;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Items.Entities
{
    public class CIdx
    {
        public const long ItemTypeId = 0;
        public const long LootRankId = 1;
        public const long Level = 2;
        public const long ScalingTypeId = 3;
        public const long EquipSlotId = 4;
        public const long BuyCost = 5;
        public const long SellValue = 6;
        public const long QualityTypeId = 7;
        public const long Max = 8;

    }

    [MessagePackObject]
    public class CrawlerSaveItem
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public string Name { get; set; }

        [Key(2)] public string Dat { get; set; }

        [Key(3)] public List<ItemEffect> Effects { get; set; } = new List<ItemEffect>();

        private long[] _dat = null;

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
