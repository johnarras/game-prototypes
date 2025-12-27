using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.Mongo
{
    public class VersionedMongoCollection<T> : MongoRepositoryCollection<T> where T : class, IStringId, IUpdateData
    {
        public VersionedMongoCollection(MongoRepository mongoRepository, ILogService logService) : base(mongoRepository, logService)
        {
        }

        protected override async Task<ReplaceOneResult> ReplaceDocument(T t, ReplaceOptions options, RepoSaveArgs args = null)
        {
            DateTime oldUpdateTime = t.UpdateTime;
            t.UpdateTime = DateTime.UtcNow;


            IClientSessionHandle session = null;

            if (args != null)
            {
                session = args.Args as IClientSessionHandle;
            }

            if (session != null)
            {
                return await _collection.ReplaceOneAsync(session, x => x.Id == t.Id && x.UpdateTime == oldUpdateTime, t, options);
            }
            else
            {
                return await _collection.ReplaceOneAsync(x => x.Id == t.Id && x.UpdateTime == oldUpdateTime, t, options);
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


