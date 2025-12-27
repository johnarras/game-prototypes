using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Entities.Assets
{
    public class EntityAtlasIcon
    {
        public string AtlasName { get; set; }
        public string IconName { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(AtlasName) && !string.IsNullOrEmpty(IconName);
        }

    }
}


