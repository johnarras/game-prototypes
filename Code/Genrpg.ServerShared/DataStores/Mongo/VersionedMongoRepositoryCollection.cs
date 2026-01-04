using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.Mongo
{
    public class VersionedMongoCollection<T> : MongoRepositoryCollection<T> where T : class, ISearchableItem, IVersionedData
    {
        public VersionedMongoCollection(MongoRepository mongoRepository, ILogService logService) : base(mongoRepository, logService)
        {
        }

        protected override async Task<ReplaceOneResult> ReplaceDocument(T t, ReplaceOptions options, RepoSaveArgs args = null)
        {
            string oldUpdateTag = t._etag;
            t._etag = Guid.NewGuid().ToString();


            IClientSessionHandle session = null;

            if (args != null)
            {
                session = args.Args as IClientSessionHandle;
            }

            if (session != null)
            {
                return await _collection.ReplaceOneAsync(session, x => x.Id == t.Id && x._etag == oldUpdateTag, t, options);
            }
            else
            {
                return await _collection.ReplaceOneAsync(x => x.Id == t.Id && x._etag == oldUpdateTag, t, options);
            }
        }

        protected override int GetMaxUpdateAttempts()
        {
            return 7;
        }

        class StubUpdateData : IVersionedData
        {
            public DateTime CreateTime { get; set; }
            public string _etag { get; set; }
        }


        string updateMemberName = null;
        protected override Dictionary<string, object> UpdateFieldNameUpdates(Dictionary<string, object> fieldNameUpdates)
        {
            if (string.IsNullOrEmpty(updateMemberName))
            {
                StubUpdateData updateData = new StubUpdateData();
                updateMemberName = nameof(updateData._etag);
            }

            if (!fieldNameUpdates.ContainsKey(updateMemberName))
            {
                fieldNameUpdates[updateMemberName] = Guid.NewGuid().ToString();
            }

            return fieldNameUpdates;
        }
    }
}


