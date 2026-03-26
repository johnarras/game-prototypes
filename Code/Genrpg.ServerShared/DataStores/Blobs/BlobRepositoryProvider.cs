using Azure.Storage.Blobs;
using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.ServerShared.DataStores.Services;
using Genrpg.ServerShared.Secrets.Services;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.Blobs
{
    public class BlobRepositoryProvider : IAzureRepositoryProvider
    {
        private ILogService _logService = null;
        private IAnalyticsService _analyticsService = null;
        private ITextSerializer _textSerializer = null;
        private ISecretsProvider _secretsProvider = null;
        public ERepoTypes HelperKey => ERepoTypes.Blob;

        public async Task<IRepository> TryCreateRepo(InitRepoArgs args, CancellationToken token)
        {
            string repoStr = args.RepoType.ToString();
            string categoryStr = args.Category.ToString();
            string secretId = repoStr + categoryStr;
            string connectionString = await _secretsProvider.GetSecret(repoStr + categoryStr);

            if (string.IsNullOrEmpty(connectionString))
            {
                return null;
            }

            BlobServiceClient client = await GetBlobClient(connectionString);

            AzureBlobRepository repo = new AzureBlobRepository();
            await repo.Init(args, client, _logService, _analyticsService, _textSerializer, token);
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
