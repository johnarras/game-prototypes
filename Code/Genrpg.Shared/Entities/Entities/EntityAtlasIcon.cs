using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Entities.Entities
{
    [MessagePackObject]
    public class EntityAtlasIcon
    {
        [Key(0)] public string AtlasName { get; set; }
        [Key(1)] public string IconName { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(AtlasName) && !string.IsNullOrEmpty(IconName);
        }

    }
}
