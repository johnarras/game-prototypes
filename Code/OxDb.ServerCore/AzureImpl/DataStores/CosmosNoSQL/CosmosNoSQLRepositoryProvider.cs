using Microsoft.Azure.Cosmos;
using OxDb.ServerCore.AzureImpl.DataStores.Entities;
using OxDb.ServerCore.AzureImpl.DataStores.Services;
using OxDb.ServerCore.Config;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;

namespace OxDb.ServerCore.AzureImpl.DataStores.CosmosNoSQL
{
    public class CosmosNoSQLRepositoryProvider : IAzureRepositoryProvider
    {
        private ILogService _logService = null;
        private ITextSerializer _textSerializer = null;
        private IServerConfig _serverConfig = null;
        public ERepoTypes HelperKey => ERepoTypes.NoSQL;

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

            CosmosNoSQLRepository repo = new CosmosNoSQLRepository();

            CosmosClient client = await GetCosmosClient(connectionString);
            await repo.Init(args, client, _logService, _textSerializer, token);
            return repo;
        }

        private static object _cosmosClientLock = new object();
        private static Dictionary<string, CosmosClient> _cosmosClientDict = new Dictionary<string, CosmosClient>();
        public async Task<CosmosClient> GetCosmosClient(string connectionString)
        {
            if (_cosmosClientDict.ContainsKey(connectionString))
            {
                return _cosmosClientDict[connectionString];
            }

            lock (_cosmosClientLock)
            {
                if (_cosmosClientDict.ContainsKey(connectionString))
                {
                    return _cosmosClientDict[connectionString];
                }

                CosmosSerializationOptions serializerOptions = new CosmosSerializationOptions()
                {
                    IgnoreNullValues = true,
                    Indented = false,
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.Default,
                };

                CosmosClientOptions options = new CosmosClientOptions()
                {
                    ConnectionMode = ConnectionMode.Direct,
                    AllowBulkExecution = false,
                    EnableContentResponseOnWrite = false,
                    SerializerOptions = serializerOptions,
                    MaxRetryAttemptsOnRateLimitedRequests = 30,
                    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(60),
                    ApplicationRegion = Regions.CentralUS,
                    RequestTimeout = TimeSpan.FromSeconds(10),
                };
                CosmosClient cosmosClient = new CosmosClient(connectionString, options);

                _cosmosClientDict[connectionString] = cosmosClient;

                return cosmosClient;
            }
        }

    }
}
