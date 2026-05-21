using OxDb.ServerCore.AzureImpl.DataStores.Mongo.Interfaces;
using OxDb.SharedCore.DataStores.DataGroups;

namespace OxDb.ServerCore.AzureImpl.DataStores.Mongo.PolymorphicNoSQL
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
