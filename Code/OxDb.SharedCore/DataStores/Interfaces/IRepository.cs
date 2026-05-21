using OxDb.SharedCore.DataStores.Entities;
using OxDb.SharedCore.Interfaces;
using System.Threading.Tasks;

namespace OxDb.SharedCore.DataStores.Interfaces
{
    public interface IRepository
    {
        Task<T> Load<T>(string id) where T : class, IStringId;
        Task<bool> Save<T>(T obj, RepoSaveArgs args = null) where T : IStringId;
        Task<bool> Delete<T>(T obj) where T : class, IStringId;
    }

}
