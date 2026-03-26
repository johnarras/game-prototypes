using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.DbQueues.Actions
{

    public class SaveAction<T> : IDbAction where T : class, IStringId
    {

        private List<T> _items { get; set; } = new List<T>();
        private IFullRepositoryService _repoService { get; set; }

        public SaveAction(T item, IFullRepositoryService repoSystem)
        {
            _repoService = repoSystem;
            _items.Add(item);
        }

        public SaveAction(List<T> items, IFullRepositoryService repoSystem)
        {
            _repoService = repoSystem;
            _items = new List<T>(items);
        }

        public async Task<bool> Execute()
        {
            return await _repoService.TransactionSave(_items).ConfigureAwait(false);
        }
    }
}


