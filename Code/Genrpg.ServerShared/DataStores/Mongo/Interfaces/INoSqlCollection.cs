using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.Mongo.Interfaces
{

    public interface INoSQLCollection
    {
        Task<object> Load(string id);
        Task<bool> TransactionSave(object t, RepoSaveArgs args = null);
        Task<bool> Save(object t, RepoSaveArgs args = null);
        Task<bool> Delete(object t);
        Task<bool> DeleteAll(object func);
        Task<bool> UpdateDict(string id, Dictionary<string, object> fieldNameUpdates);
        Task<bool> UpdateAction(string id, object action);
        Task<bool> SaveAll(object itemList);
        Task<object> AtomicIncrement(string docId, string fieldName, long increment);
        Task<object> AtomicAddBits(string docId, string fieldName, long addBits);
        Task<object> AtomicRemoveBits(string docId, string fieldName, long removeBits);
        Task CreateIndex(CreateIndexData options);
    }

    public interface ITypedNoSQLCollection<T> : INoSQLCollection where T : class, ISearchableItem, IStringId
    {
        Task<List<T>> Search(object func, int quantity = 1000, int skip = 0);
    }

}
