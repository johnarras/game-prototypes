using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using OxDb.ServerCore.AzureImpl.DataStores.Entities;
using OxDb.ServerCore.AzureImpl.DataStores.Mongo.Interfaces;
using OxDb.ServerCore.AzureImpl.DataStores.Services;
using OxDb.ServerCore.Config;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using System.Security.Authentication;

namespace OxDb.ServerCore.AzureImpl.DataStores.Mongo
{
    /// <summary>
    /// This creates mongo repos.
    /// This is intentionally not generic so that the static connection pool doesn't make the same connection
    /// more than once. If you want, you can make this generic and move the pool object outside of this class.
    /// </summary>
    public abstract class BaseMongoRepositoryProvider : IAzureRepositoryProvider
    {
        private ILogService _logService = null;
        private ITextSerializer _textSerializer = null;
        private IReflectionService _reflectionService = null;
        private IServerConfig _serverConfig = null;
        public abstract ERepoTypes HelperKey { get; }
        protected abstract IMongoInitRepository CreateRepository();

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

            MongoClient client = await GetMongoClient(connectionString);

            IMongoInitRepository repo = CreateRepository();
            await repo.Init(args, client, _logService, _textSerializer, _reflectionService, token);
            return repo;
        }

        private static object _mongoClientLock = new object();
        private static Dictionary<string, MongoClient> _mongoClientDict = new Dictionary<string, MongoClient>();

        protected async Task<MongoClient> GetMongoClient(string connectionString)
        {
            if (_mongoClientDict.ContainsKey(connectionString))
            {
                return _mongoClientDict[connectionString];
            }

            lock (_mongoClientLock)
            {
                if (_mongoClientDict.ContainsKey(connectionString))
                {
                    return _mongoClientDict[connectionString];
                }

                if (_mongoClientDict.Keys.Count == 0)
                {
                    ConventionRegistry.Register("IgnoreMessyData",
                                    new ConventionPack
                                    {
                                new IgnoreIfDefaultConvention(true),
                                new IgnoreExtraElementsConvention(true),
                                    },
                                    t => true);
                }

                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
                settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
                settings.RetryWrites = false;
                settings.RetryReads = true;
                MongoClient mongoClient = new MongoClient(settings);
                _mongoClientDict[connectionString] = mongoClient;

                return mongoClient;
            }
        }
    }
}
