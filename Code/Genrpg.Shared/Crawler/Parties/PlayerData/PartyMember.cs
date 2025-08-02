using Genrpg.Shared.Crawler.Items.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Utils.Data;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{

    // MessagePackIgnore
    public class StatRegenFraction
    {
        [Key(0)] public long StatTypeId { get; set; }
        [Key(1)] public float Fraction { get; set; }
    }

    // MessagePackIgnore
    public class PartyMember : CrawlerUnit
    {
       public int PartySlot { get; set; }

        [JsonIgnore]
        public List<Item> Equipment { get; set; } = new List<Item>();

        public List<CrawlerSaveItem> SaveEquipment { get; set; } = new List<CrawlerSaveItem>();

        public string PermStats { get; set; }

        public const long PermStatSize = StatConstants.PrimaryStatEnd - StatConstants.PrimaryStatStart + 1;

        private long[] _permStats { get; set; } = new long[PermStatSize];

        public List<StatRegenFraction> RegenFractions { get; set; } = new List<StatRegenFraction>();

        public SmallIdShortCollection Upgrades { get; set; } = new SmallIdShortCollection();

        public void ClearPermStats()
        {
            _permStats = new long[PermStatSize];
        }

        public void ConvertDataAfterLoad()
        {
            if (!string.IsNullOrEmpty(PermStats))
            {
                string[] words = PermStats.Split(' ');  

                for (int i = 0; i < words.Length && i < _permStats.Length; i++)
                {
                    if (Int64.TryParse(words[i], out long val))
                    {
                        _permStats[i] = val;
                    }
                }
                if (words.Length > _permStats.Length)
                {
                    if (Int64.TryParse(words[_permStats.Length], out long health))
                    {
                        Stats.Set(StatTypes.Health, StatCategories.Curr, health);
                    }
                }
                if (words.Length > _permStats.Length+1)
                {
                    if (Int64.TryParse(words[_permStats.Length+1], out long health))
                    {
                        Stats.Set(StatTypes.Mana, StatCategories.Curr, health);
                    }
                }
            }
        }

        public void ConvertDataBeforeSave()
        {
            StringBuilder sb = new StringBuilder();
            for (int i =0; i < _permStats.Length; i++)
            {
                sb.Append(_permStats[i].ToString());
                if (i < _permStats.Length - 1)
                {
                    sb.Append(" ");
                }
            }
            sb.Append(" " + Stats.Curr(StatTypes.Health));
            sb.Append(" " + Stats.Curr(StatTypes.Mana));
            PermStats = sb.ToString();  
        }


        public long Exp { get; set; }

        public int UpgradePoints { get; set; }

        public List<PartySummon> Summons { get; set; } = new List<PartySummon>();

        public long WarpMapId { get; set; }
        public int WarpMapX { get; set; }
        public int WarpMapZ { get; set; }
        public int WarpRot { get; set; }
        public long LastCombatCrawlerSpellId { get; set; }

        public override bool IsPlayer() { return true; }

        public long GetPermStat(long statTypeId)
        {
            return _permStats[statTypeId-StatConstants.PrimaryStatStart];          
        }

        public void SetPermStat(long statTypeId, long val)
        {
            _permStats[statTypeId-StatConstants.PrimaryStatStart] = val;
        }

        public void AddPermStat(long statTypeId, long val)
        {
            _permStats[statTypeId - StatConstants.PrimaryStatStart] += val;
        }

        public override Item GetEquipmentInSlot(long equipSlotId)
        {
            return Equipment.FirstOrDefault(x=>x.EquipSlotId == equipSlotId);
        }

        protected override bool AlwaysCreateMissingData() { return true; }
    }
}
