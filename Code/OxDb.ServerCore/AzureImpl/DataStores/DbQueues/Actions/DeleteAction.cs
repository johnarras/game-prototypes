using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;

namespace OxDb.ServerCore.AzureImpl.DataStores.DbQueues.Actions
{
    public class DeleteAction<T> : IDbAction where T : class, IStringId
    {
        private T _obj { get; set; }
        private IRepositoryService _repoSystem { get; set; }

        public DeleteAction(T item, IRepositoryService repoSystem)
        {
            _obj = item;
            _repoSystem = repoSystem;
        }

        public async Task<bool> Execute()
        {
            return await _repoSystem.Delete(_obj).ConfigureAwait(false);
        }
    }
}


