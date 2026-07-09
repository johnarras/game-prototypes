using MessagePack;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Core;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild
{
    [DataGroup(EDataCategories.Players, ERepoTypes.Mongo)]
    public abstract class OwnerObjectList<TChild> : BasePlayerData, ITopLevelUnitData, ISearchableItem, IVersionedData where TChild : OwnerPlayerData
    {

        [MessagePack.IgnoreMember] public abstract string VersionTag { get; set; }
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


