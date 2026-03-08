using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.ServerShared.DataStores.Mongo.Interfaces;
using Genrpg.ServerShared.DataStores.Services;
using Genrpg.ServerShared.Secrets.Services;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using MongoDB.Driver.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
