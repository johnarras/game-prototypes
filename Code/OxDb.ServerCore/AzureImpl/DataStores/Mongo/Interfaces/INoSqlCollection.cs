using OxDb.SharedCore.DataStores.Entities;
using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;

namespace OxDb.ServerCore.AzureImpl.DataStores.Mongo.Interfaces
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
