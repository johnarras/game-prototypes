using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class ItemNameResult
    {
        [Key(0)] public string SingularName { get; set; }
        [Key(1)] public string PluralName { get; set; }
    }
}
