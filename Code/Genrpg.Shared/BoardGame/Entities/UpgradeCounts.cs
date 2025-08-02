using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Entities
{
    [MessagePackObject]
    public class UpgradeCounts
    {
        [Key(0)] public bool IsOwnBoard { get; set; }
        [Key(1)] public int TotalUpgrades { get; set; }
        [Key(2)] public int CurrUpgrades { get; set; }
        [Key(3)] public int UpgradeTileTypeCount { get; set; }
        [Key(4)] public int UpgradeTiers { get; set; }
    }
}
