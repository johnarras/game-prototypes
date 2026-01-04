using Genrpg.Shared.DataStores.Categories.PlayerData.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.NoChild
{
    // Cannot add repo attribute here since the shared and private data split into different areas
    public abstract class NoChildPlayerData : BasePlayerData, ITopLevelUnitData
    {
        public override IUnitData Unpack() { return this; }
        [IgnoreMember] public string _etag { get; set; }

        public List<IUnitData> GetChildren() { return new List<IUnitData>(); }

    }
}


