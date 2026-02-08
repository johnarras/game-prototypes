using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.ServerShared.DataStores.Services;
using Genrpg.ServerShared.Secrets.Services;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.CosmosNoSQL
{
    public class CosmosNoSQLRepositoryProvider : IAzureRepositoryProvider
    {
        private ILogService _logService = null;
        private IAnalyticsService _analyticsService = null;
        private ITextSerializer _textSerializer = null;
        private ISecretsProvider _secretsProvider = null;
        public ERepoTypes HelperKey => ERepoTypes.NoSQL;

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

            CosmosNoSQLRepository repo = new CosmosNoSQLRepository();

            CosmosClient client = await GetCosmosClient(connectionString);
            await repo.Init(args, client, _logService, _analyticsService, _textSerializer, token);
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
                };
                CosmosClient cosmosClient = new CosmosClient(connectionString, options);

                _cosmosClientDict[connectionString] = cosmosClient;

                return cosmosClient;
            }
        }

    }
}
