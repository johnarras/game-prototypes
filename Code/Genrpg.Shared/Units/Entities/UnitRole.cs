using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Units.Entities
{
    [MessagePackObject]
    public class UnitRole
    {
        [Key(0)] public long RoleId { get; set; }
        [Key(1)] public int Level { get; set; }
    }
}
