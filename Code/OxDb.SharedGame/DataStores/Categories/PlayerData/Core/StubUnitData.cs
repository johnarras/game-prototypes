using MessagePack;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using System.Collections.Generic;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.Core
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


