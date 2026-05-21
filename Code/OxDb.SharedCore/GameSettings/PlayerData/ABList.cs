using MessagePack;
using System;
using System.Collections.Generic;

namespace OxDb.SharedCore.GameSettings.PlayerData
{
    [MessagePackObject]
    public class ABItem
    {
        [Key(0)] public long SettingsId { get; set; }
        [Key(1)] public string DocId { get; set; }
    }
    [MessagePackObject]
    public class ABList
    {
        [Key(0)] public List<ABItem> Items { get; set; } = new List<ABItem>();

        [Key(1)] public DateTime CheckTime { get; set; } = DateTime.UtcNow;
        [Key(2)] public DateTime SetTime { get; set; } = DateTime.UtcNow;
    }
}


