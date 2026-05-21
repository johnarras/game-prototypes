using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OxDb.SharedCore.DataStores.Interfaces
{
    public interface ISearchRepositoryService : IRepositoryService
    {
        Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity = 1000, int skip = 0) where T : class, ISearchableItem; // LoadAll
    }
}
