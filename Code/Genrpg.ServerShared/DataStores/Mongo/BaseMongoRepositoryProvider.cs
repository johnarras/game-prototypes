using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.ServerShared.DataStores.Mongo.Interfaces;
using Genrpg.ServerShared.DataStores.Services;
using Genrpg.ServerShared.Secrets.Services;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.Mongo.PolymorphicNoSQL
{
    public abstract class BaseMongoRepositoryProvider<MR> : IAzureRepositoryProvider where MR : class, IMongoInitRepository, new()
    {
        private ILogService _logService = null;
        private IAnalyticsService _analyticsService = null;
        private ITextSerializer _textSerializer = null;
        private ISecretsProvider _secretsProvider = null;
        public abstract ERepoTypes HelperKey { get; }

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

            MongoClient client = await GetMongoClient(connectionString);

            MR repo = new MR();
            await repo.Init(args, client, _logService, _analyticsService, _textSerializer, token);
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
                MongoClient mongoClient = new MongoClient(settings);
                _mongoClientDict[connectionString] = mongoClient;

                return mongoClient;
            }
        }
    }
}
