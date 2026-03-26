using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;
using Genrpg.Shared.Effects.Entities;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Inventory.PlayerData
{
    [MessagePackObject]
    public class Item : OwnerPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public long ItemTypeId { get; set; }
        [Key(4)] public long Level { get; set; }
        [Key(5)] public List<Effect> Effects { get; set; } = new List<Effect>();
        [Key(6)] public long LootRankId { get; set; }

        [Key(7)] public int IconIndex { get; set; }

        [Key(8)] public long EquipSlotId { get; set; }
        [Key(9)] public long BuyCost { get; set; }
        [Key(10)] public long SellValue { get; set; }

        [Key(11)] public List<ItemProc> Procs { get; set; } = new List<ItemProc>();


        private string _art;
        public string GetArt() { return _art; }
        public void SetArt(string art) { _art = art; }

        private string _basicInfo;
        public string GetBasicInfo() { return _basicInfo; }
        public void SetBasicInfo(string basicInfo) { _basicInfo = basicInfo; }

        public Item()
        {
            Effects = new List<Effect>();
        }

    }
}


