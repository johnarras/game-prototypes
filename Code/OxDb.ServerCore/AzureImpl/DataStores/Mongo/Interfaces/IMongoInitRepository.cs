using MongoDB.Driver;
using OxDb.ServerCore.AzureImpl.DataStores.Entities;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;

namespace OxDb.ServerCore.AzureImpl.DataStores.Mongo.Interfaces
{
    public interface IMongoInitRepository : IRepository
    {
        Task Init(InitRepoArgs args,
             MongoClient client,
             ILogService logService,
             ITextSerializer serializer,
             IReflectionService reflectionService,
             CancellationToken token);
    }
}
