using Genrpg.ServerShared.DataStores.Mongo.Interfaces;
using Genrpg.Shared.DataStores.DataGroups;

namespace Genrpg.ServerShared.DataStores.Mongo.PolymorphicNoSQL
{
    public class PolymorphicMongoRepositoryProvider : BaseMongoRepositoryProvider
    {
        public override ERepoTypes HelperKey => ERepoTypes.Polymorphic;

        protected override IMongoInitRepository CreateRepository()
        {
            return new PolymorphicMongoRepository();
        }
    }
}
