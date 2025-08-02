using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.WhoList.Entities
{
    [MessagePackObject]
    public class WhoListItem
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string ZoneName { get; set; }
        [Key(3)] public int Level { get; set; }
    }
}
