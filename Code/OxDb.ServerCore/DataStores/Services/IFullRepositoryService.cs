using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using System.Linq.Expressions;

namespace OxDb.ServerCore.DataStores.Services
{
    public interface IFullRepositoryService : ISearchRepositoryService
    {
        Task<T> AtomicIncrement<T>(string docId, string fieldName, long increment) where T : class, IStringId;
        Task<T> AtomicAddBits<T>(string docId, string fieldName, long addBits) where T : class, IStringId;
        Task<T> AtomicRemoveBits<T>(string docId, string fieldName, long removeBits) where T : class, IStringId;
        void QueueDelete<T>(T t) where T : class, IStringId;
        void QueueSave<T>(T t) where T : class, IStringId;

        Task<bool> UpdateDict<T>(string docId, Dictionary<string, object> fieldNameUpdates) where T : class, IStringId;
        void QueueUpdateDict<T>(string docId, Dictionary<string, object> fieldNameUpdates) where T : class, IStringId;

        Task<bool> UpdateAction<T>(string docId, Action<T> action) where T : class, IStringId;
        void QueueUpdateAction<T>(string docId, Action<T> action) where T : class, IStringId;

        Task<bool> SaveAll<T>(List<T> list) where T : class, IStringId;
        Task<bool> DeleteAll<T>(Expression<Func<T, bool>> func) where T : class, IStringId;
        Task CreateIndexes(CreateIndexData data);
        Task<bool> TransactionSave<T>(List<T> list) where T : class, IStringId;
        void QueueTransactionSave<T>(List<T> list, string queueId) where T : class, IStringId;


    }


}
