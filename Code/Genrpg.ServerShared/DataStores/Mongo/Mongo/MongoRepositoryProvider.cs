using Genrpg.ServerShared.DataStores.Mongo.Interfaces;
using Genrpg.Shared.DataStores.DataGroups;

namespace Genrpg.ServerShared.DataStores.Mongo.Mongo
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
