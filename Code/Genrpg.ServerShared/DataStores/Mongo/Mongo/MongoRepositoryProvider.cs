using Genrpg.ServerShared.DataStores.Mongo.PolymorphicNoSQL;
using Genrpg.Shared.DataStores.DataGroups;

namespace Genrpg.ServerShared.DataStores.Mongo.Mongo
{
    public class MongoRepositoryProvider : BaseMongoRepositoryProvider<MongoRepository>
    { 
        public override ERepoTypes HelperKey => ERepoTypes.Mongo;
    }
}
