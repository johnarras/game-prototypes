using Genrpg.Shared.DataStores.Categories.ContentData;
using Genrpg.Shared.DataStores.Constants;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Core.PlayerData
{
    public class PublicUser : BaseGameContentData
    {
        public override string Id { get; set; }
        public string Name { get; set; }

    }
}


