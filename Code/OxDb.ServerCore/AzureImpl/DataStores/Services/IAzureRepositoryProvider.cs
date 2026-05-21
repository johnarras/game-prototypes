using OxDb.ServerCore.AzureImpl.DataStores.Entities;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;

namespace OxDb.ServerCore.AzureImpl.DataStores.Services
{
    public interface IAzureRepositoryProvider : ISetupDictionaryItem<ERepoTypes>
    {
        public Task<IRepository> TryCreateRepo(InitRepoArgs args, CancellationToken token);
    }
}
