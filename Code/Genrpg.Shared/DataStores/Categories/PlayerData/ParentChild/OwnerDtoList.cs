using Genrpg.Shared.DataStores.Categories.PlayerData.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using System.Collections.Generic;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild
{
    public abstract class OwnerDtoList<TParent, TChild> : StubUnitData
        where TParent : OwnerObjectList<TChild>, new()
        where TChild : OwnerPlayerData
    {
        [MessagePack.IgnoreMember] public abstract List<TChild> Children { get; set; }
        [MessagePack.IgnoreMember] public abstract TParent Parent { get; set; }

        public override IUnitData Unpack()
        {
            Parent.SetData(Children);
            return Parent;
        }
    }
}


