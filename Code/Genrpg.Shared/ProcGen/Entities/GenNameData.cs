using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Entities
{
    public class GenNameData
    {
        public string QualityList { get; set; }
        public string LevelList { get; set; }
        public string Suffix { get; set; }
        public int ItemCategoryId { get; set; }
        public bool UseAAnSuffix { get; set; }
        public int QualityUpgradeCost { get; set; }
        public int MinLevelUpgradeCost { get; set; }
        public int MaxLevelUpgradeCost { get; set; }
        public bool GenNameIsSuffix { get; set; }
        public int MaxNumtoUse { get; set; }
        public bool AllItemsHaveAllLevels { get; set; }
        public int CategorySpawnTableId { get; set; }

        public GenNameData()
        {
            QualityList = "";
            LevelList = "";
            Suffix = "";
            ItemCategoryId = 0;
            UseAAnSuffix = false;
            QualityUpgradeCost = 0;
            MinLevelUpgradeCost = 0;
            MaxLevelUpgradeCost = 0;
            GenNameIsSuffix = true;
            MaxNumtoUse = 0;
            AllItemsHaveAllLevels = false;
            CategorySpawnTableId = 0;
        }

    }
}


