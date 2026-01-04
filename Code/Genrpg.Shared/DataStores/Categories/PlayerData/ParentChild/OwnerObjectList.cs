using Genrpg.Shared.DataStores.Categories.PlayerData.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Interfaces;
using MessagePack;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild
{
    [DataGroup(EDataCategories.Players, ERepoTypes.Mongo)]
    public abstract class OwnerObjectList<TChild> : BasePlayerData, ITopLevelUnitData, ISearchableItem where TChild : OwnerPlayerData
    {
        [IgnoreMember] public string _etag { get; set; }
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

        public List<IUnitData> GetChildren()
        {
            return _data.Cast<IUnitData>().ToList();
        }
    }
}


