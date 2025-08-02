using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.NoSQL
{
    public class VersionedNoSQLCollection<T> : MongoNoSQLRepositoryCollection<T> where T : class, IStringId, IUpdateData
    {
        public VersionedNoSQLCollection(AzureCosmosMongoRepository mongoRepository, ILogService logService) : base(mongoRepository, logService)
        {
        }

        protected override async Task<ReplaceOneResult> ReplaceDocument(T t, ReplaceOptions options, IClientSessionHandle session)
        {
            DateTime oldUpdateTime = t.UpdateTime;
            t.UpdateTime = DateTime.UtcNow;
            if (session != null)
            {
                return await _collection.ReplaceOneAsync(session, x => x.Id == t.Id && (x.UpdateTime == oldUpdateTime), t, options);
            }
            else
            {
                return await _collection.ReplaceOneAsync(x => x.Id == t.Id && (x.UpdateTime == oldUpdateTime), t, options);
            }
        }

        protected override int GetMaxUpdateAttempts()
        {
            return 7;
        }

        class StubUpdateData : IUpdateData
        {
            public DateTime CreateTime { get; set; }
            public DateTime UpdateTime { get; set; }
        }


        string updateMemberName = null;
        protected override Dictionary<string, object> UpdateFieldNameUpdates(Dictionary<string, object> fieldNameUpdates)
        {
            if (string.IsNullOrEmpty(updateMemberName))
            {
                StubUpdateData updateData = new StubUpdateData();
                updateMemberName = nameof(updateData.UpdateTime);
            }

            if (!fieldNameUpdates.ContainsKey(updateMemberName))
            {
                fieldNameUpdates[updateMemberName] = DateTime.UtcNow;
            }

            return fieldNameUpdates;
        }
    }
}
