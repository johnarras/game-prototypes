using OxDb.SharedCore.DataStores.Entities;
using OxDb.SharedCore.Interfaces;
using System.Threading.Tasks;

namespace OxDb.SharedCore.DataStores.Interfaces
{
    public interface IRepositoryService : IPriorityInitializable
    {
        Task<T> Load<T>(string id) where T : class, IStringId;

        Task<bool> Save<T>(T t, RepoSaveArgs args = null) where T : IStringId;

        Task<bool> Delete<T>(T t) where T : class, IStringId;
    }
}
