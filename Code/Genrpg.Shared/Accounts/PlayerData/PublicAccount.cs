using Genrpg.Shared.DataStores.Categories.ContentData;
using Genrpg.Shared.DataStores.Constants;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Accounts.PlayerData
{
    public class PublicAccount : BaseAccountContentData
    {
        public override string Id { get; set; }
        public string Name { get; set; }

    }
}


