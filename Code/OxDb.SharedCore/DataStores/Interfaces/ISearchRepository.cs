using OxDb.SharedCore.DataStores.Indexes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedCore.DataStores.Interfaces
{
    public interface ISearchRepository : IRepository
    {
        Task<List<T>> Search<T>(object func, int quantity = 1000, int skip = 0) where T : class, ISearchableItem;
        Task CreateIndexes(CreateIndexData data);
    }
}
