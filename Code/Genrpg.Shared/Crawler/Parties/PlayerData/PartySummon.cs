using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    public class PartySummon
    { 
        public long UnitTypeId { get; set; }
        public string Name { get; set; }
        public long RoleId { get; set; }
    }
}


