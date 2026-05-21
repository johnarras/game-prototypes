using Azure.Storage.Blobs;
using OxDb.ServerCore.AzureImpl.DataStores.Entities;
using OxDb.ServerCore.AzureImpl.DataStores.Services;
using OxDb.ServerCore.Config;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;

namespace OxDb.ServerCore.AzureImpl.DataStores.Blobs
{
    public class BlobRepositoryProvider : IAzureRepositoryProvider
    {
        private ILogService _logService = null;
        private ITextSerializer _textSerializer = null;
        private IServerConfig _serverConfig = null;
        public ERepoTypes HelperKey => ERepoTypes.Blob;

        public async Task<IRepository> TryCreateRepo(InitRepoArgs args, CancellationToken token)
        {
            string repoStr = args.RepoType.ToString();
            string categoryStr = args.Category.ToString();
            string secretId = repoStr + categoryStr;
            string connectionString = _serverConfig.GetConfigVal(repoStr + categoryStr);

            if (string.IsNullOrEmpty(connectionString))
            {
                return null;
            }

            BlobServiceClient client = await GetBlobClient(connectionString);

            AzureBlobRepository repo = new AzureBlobRepository();
            await repo.Init(args, client, _logService, _textSerializer, token);
            return repo;

        }
        private static object _blobContainerLock = new object();
        private Dictionary<string, BlobServiceClient> _blobClients = new Dictionary<string, BlobServiceClient>();

        public async Task<BlobServiceClient> GetBlobClient(string connectionString)
        {
            if (_blobClients.ContainsKey(connectionString))
            {
                return _blobClients[connectionString];
            }

            lock (_blobContainerLock)
            {
                if (_blobClients.ContainsKey(connectionString))
                {
                    return _blobClients[connectionString];
                }
                BlobServiceClient serviceClient = new BlobServiceClient(connectionString);

                _blobClients[connectionString] = serviceClient;

                return serviceClient;
            }
        }
    }
}
