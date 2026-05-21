using OxDb.SharedGame.DataStores.Categories.PlayerData.Core;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using System.Collections.Generic;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild
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


