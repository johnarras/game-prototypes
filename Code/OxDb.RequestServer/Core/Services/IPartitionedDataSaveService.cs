using OxDb.ServerCore.AzureImpl.DataStores.CosmosNoSQL;
using OxDb.ServerCore.DataStores.Services;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.DataStores.Interfaces;

namespace OxDb.RequestServer.Core.Services
{
    public interface IPartitionedDataSaveService : IInjectable
    {
        Task<bool> SavePartitionedList<T>(List<T> data, IRepositoryService repoService) where T : IPartitionedData;
    }

    public class PartitionedDataSaveService : IPartitionedDataSaveService
    {
        public async Task<bool> SavePartitionedList<T>(List<T> data, IRepositoryService repoService) where T : IPartitionedData
        {
            if (data.Count < 1)
            {
                return true;
            }

            // This is ugly but I am trying to keep these specific downcasts contained
            // in one spot for this special situation.
            FullRepositoryService fullService = repoService as FullRepositoryService;

            CosmosNoSQLRepository cosmosRepo = fullService.FindRepo(data[0].GetType()) as CosmosNoSQLRepository;

            return await cosmosRepo.TransactionSave(data);
        }
    }
}
