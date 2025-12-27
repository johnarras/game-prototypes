using System;

namespace Genrpg.Editor.Importers.PlayerFiltering
{
    public class PlayerFilterImportRow
    {
        public bool Enabled { get; set; }
        public long TotalModSize { get; set; }
        public long MaxModValue { get; set; }
        public long MinLevel { get; set; }
        public long MaxLevel { get; set; }
        public long Priority { get; set; }
        public double MinInstallDays { get; set; }
        public double MaxInstallDays { get; set; }
        public long MinPurchaseCount { get; set; }
        public long MaxPurchaseCount { get; set; }
        public double MinPurchaseTotal { get; set; }
        public double MaxPurchaseTotal { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RepeatHours { get; set; }
        public bool RepeatMonthly { get; set; }
        public Version MinClientVersion { get; set; }
        public Version MaxClientVersion { get; set; }
    }
}


