using OxDb.ServerCore.AzureImpl.DataStores.Mongo.Interfaces;
using OxDb.SharedCore.DataStores.DataGroups;

namespace OxDb.ServerCore.AzureImpl.DataStores.Mongo.Mongo
{
    public class MongoRepositoryProvider : BaseMongoRepositoryProvider
    {
        public override ERepoTypes HelperKey => ERepoTypes.Mongo;

        protected override IMongoInitRepository CreateRepository()
        {
            return new MongoRepository();
        }
    }
}
