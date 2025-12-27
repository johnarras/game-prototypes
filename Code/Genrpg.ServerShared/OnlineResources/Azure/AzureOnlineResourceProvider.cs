using Genrpg.ServerShared.CloudComms.Platforms;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.DataStores.Blobs;
using Genrpg.ServerShared.DataStores.CosmosNoSQL;
using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.ServerShared.DataStores.Mongo;
using Genrpg.ServerShared.OnlineResources.Interfaces;
using Genrpg.ServerShared.Secrets.Services;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Tasks.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.OnlineResources.Azure
{

    public class AzureOnlineResourceProvider : IOnlineResourceProvider
    {
        private ILogService _logService = null;
        private IAnalyticsService _analyticsService = null;
        private ITextSerializer _textSerializer = null;
        private ISecretsProvider _secretsProvider = null;

        public async Task<ICloudMessageImpl> CreateCloudMessageImpl(IServiceLocator loc, IServerConfig config, ILogService logService,
            ITextSerializer serializer, ISecretsProvider secretsProvider, ITaskService taskService, ICloudCommsService cloudCommsService, CancellationToken token)
        {
            AzureCloudMessageImpl commsImpl = new AzureCloudMessageImpl();
            await commsImpl.Init(loc, config, logService, serializer, secretsProvider, taskService, cloudCommsService, token);
            return commsImpl;
        }

        public async Task<IRepository> CreateRepo(InitRepoArgs args, CancellationToken token)
        {
            string repoStr = args.RepoType.ToString();
            string categoryStr = args.Category.ToString();

            string secretId = repoStr + categoryStr;
            string connectionString = await _secretsProvider.GetSecret(repoStr + categoryStr);

            if (args.RepoType == ERepoTypes.Mongo)
            {
                MongoRepository repo = new MongoRepository();
                await repo.Init(args, connectionString, _logService, _analyticsService, _textSerializer);
                return repo;
            }
            else if (args.RepoType == ERepoTypes.Blob)
            {
                AzureBlobRepository repo = new AzureBlobRepository();
                await repo.Init(args, connectionString, _logService, _analyticsService, _textSerializer, token);
                return repo;
            }
            else if (args.RepoType == ERepoTypes.NoSQL)
            {
                CosmosNoSQLRepository repo = new CosmosNoSQLRepository();
                await repo.Init(args, connectionString, _logService, _analyticsService, _textSerializer, token);
                return repo;
            }

            return null;
        }

        public string GetPublicIPAddress(IServerConfig config, ILogService logService, ISecretsProvider secretsProvider, CancellationToken token)
        {

            return "";
        }

    }
}



