using MessagePack;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using Genrpg.Shared.DataStores.Categories.PlayerData.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild
{
    [MessagePackObject]
    public class OwnerDtoList<TParent, TChild> : StubUnitData
        where TParent : OwnerObjectList<TChild>, new()
        where TChild : OwnerPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<TChild> Children { get; set; } = new List<TChild>();
        [Key(2)] public TParent Parent { get; set; }

        public override IUnitData Unpack()
        {
            Parent.SetData(Children);
            return Parent;
        }
    }
}
