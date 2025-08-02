using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    [MessagePackObject]
    public class PartySummon
    { 
        [Key(0)] public long UnitTypeId { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public long RoleId { get; set; }
    }
}
