using Genrpg.Shared.DataStores.Categories.PlayerData.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using MessagePack;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild
{
    public abstract class OwnerObjectList<TChild> : BasePlayerData, ITopLevelUnitData where TChild : OwnerPlayerData
    {
        [IgnoreMember] public DateTime UpdateTime { get; set; }
        protected List<TChild> _data = new List<TChild>();
        public virtual void SetData(List<TChild> data)
        {
            _data = data;
        }

        public virtual IReadOnlyList<TChild> GetData()
        {
            return _data;
        }

        public override IUnitData Unpack() { return this; }
    }
}
