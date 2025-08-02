using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.Core
{
    // MessagePackIgnore
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
