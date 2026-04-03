using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Utils;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.Mongo.Interfaces
{
    public interface IMongoInitRepository : IRepository
    {
        Task Init(InitRepoArgs args,
             MongoClient client,
             ILogService logService,
             IAnalyticsService analyticsService,
             ITextSerializer serializer,
             IReflectionService reflectionService,
             CancellationToken token);
    }
}
