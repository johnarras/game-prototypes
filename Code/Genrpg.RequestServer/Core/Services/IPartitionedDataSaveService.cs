using Genrpg.ServerShared.DataStores;
using Genrpg.ServerShared.DataStores.CosmosNoSQL;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Core.Services
{
    public interface IPartitionedDataSaveService : IInjectable
    {
        Task SavePartitionedList<T>(List<T> data, IRepositoryService repoService) where T : IPartitionedData;
    }

    public class PartitionedDataSaveService : IPartitionedDataSaveService
    {
        public async Task SavePartitionedList<T>(List<T> data, IRepositoryService repoService) where T : IPartitionedData
        {
            if (data.Count < 1)
            {
                return;
            }

            // This is ugly but I am trying to keep these specific downcasts contained
            // in one spot for this special situation.
            FullRepositoryService fullService = repoService as FullRepositoryService;

            CosmosNoSQLRepository cosmosRepo = fullService.FindRepo(data[0].GetType()) as CosmosNoSQLRepository;

            await cosmosRepo.TransactionSave(data);
        }
    }
}
