using Genrpg.Shared.DataStores.Categories.ContentData;
using Genrpg.Shared.DataStores.Constants;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Characters.PlayerData
{
    public class PublicCharacter : BaseGameContentData
    {
        public override string Id { get; set; }
        public string Name { get; set; }
        public long FactionTypeId { get; set; }
        public long UnitTypeId { get; set; }
        public long SexTypeId { get; set; }

    }
}


