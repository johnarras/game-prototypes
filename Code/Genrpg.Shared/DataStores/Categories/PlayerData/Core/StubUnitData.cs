using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Entities;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.Core
{
    public abstract class StubUnitData : IUnitData
    {
        [IgnoreMember] public abstract string Id { get; set; }

        public virtual IUnitData Unpack() { return this; }

        public void QueueDelete(IRepositoryService repoService) { }
        public void QueueSave(IRepositoryService repoService) { }

        public virtual List<IUnitData> GetChildren() { return new List<IUnitData>(); }



        public List<BasePlayerData> GetSaveObjects(bool saveClean)
        {
            return new List<BasePlayerData>();
        }
    }
}


