using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.Services
{
    public interface IAzureRepositoryProvider : ISetupDictionaryItem<ERepoTypes>
    {
        public Task<IRepository> TryCreateRepo(InitRepoArgs args, CancellationToken token);
    }
}
