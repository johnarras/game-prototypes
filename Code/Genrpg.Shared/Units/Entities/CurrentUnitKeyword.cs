using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Units.Entities
{
    [MessagePackObject]
    public class CurrentUnitKeyword
    {
        [Key(0)] public long UnitKeywordId { get; set; }
    }

}
