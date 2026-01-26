using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace Genrpg.Shared.DataStores.Entities
{
    public enum RepositoryCategory
    {
        None = 0,
        File = 1,
        Sql = 2,
        World = 3,
        GameData = 4,
    };

    public interface IRepositoryService : IPriorityInitializable
    {
        Task<T> Load<T>(string id) where T : class, IStringId;

        Task<bool> Save<T>(T t, RepoSaveArgs args = null) where T : IStringId;

        Task<bool> Delete<T>(T t) where T : class, IStringId;
    }


    public interface ISearchRepositoryService : IRepositoryService
    {
        Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity = 1000, int skip = 0) where T : class, ISearchableItem; // LoadAll
    }

    public interface IRepository
    {
        Task<T> Load<T>(string id) where T : class, IStringId;
        Task<bool> Save<T>(T obj, RepoSaveArgs args = null) where T : IStringId;
        Task<bool> Delete<T>(T obj) where T : class, IStringId;
    }

    public interface ISearchRepository : IRepository
    {
        Task<List<T>> Search<T>(object func, int quantity = 1000, int skip = 0) where T : class, ISearchableItem;
    }

    public class RepoSaveArgs
    {
        public bool Verbose { get; set; }
        public object Args { get; set; }
    }
}

