using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using System.Linq.Expressions;

namespace OxDb.ServerCore.DataStores.Services
{
    public interface IFullRepository : ISearchRepository
    {
        Task<T> AtomicIncrement<T>(string docId, string fieldName, long increment) where T : class, IStringId;
        Task<T> AtomicAddBits<T>(string docId, string fieldName, long addBits) where T : class, IStringId;
        Task<T> AtomicRemoveBits<T>(string docId, string fieldName, long removeBits) where T : class, IStringId;
        Task<bool> DeleteAll<T>(Expression<Func<T, bool>> func) where T : class, IStringId;
        Task<bool> UpdateDict<T>(string docId, Dictionary<string, object> fieldNameUpdates) where T : class, IStringId;
        Task<bool> UpdateAction<T>(string docId, Action<T> action) where T : class, IStringId;

        Task<bool> SaveAll<T>(List<T> tlist) where T : class, IStringId;
        Task<bool> TransactionSave<T>(List<T> list) where T : class, IStringId;
    }
}
