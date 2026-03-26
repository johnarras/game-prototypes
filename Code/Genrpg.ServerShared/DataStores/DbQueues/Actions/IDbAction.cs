using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.DbQueues.Actions
{
    public interface IDbAction
    {
        Task<bool> Execute();
    }
}


