using MongoDB.Driver;
using OxDb.SharedCore.DataStores.Entities;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;

namespace OxDb.ServerCore.AzureImpl.DataStores.Mongo.Mongo
{
    public class VersionedMongoCollection<T> : MongoRepositoryCollection<T> where T : class, ISearchableItem, IVersionedData
    {
        public VersionedMongoCollection(IMongoDatabase mongoDatabase, ILogService logService) : base(mongoDatabase, logService)
        {
        }



        private ReplaceOptions _casReplaceOptions = new ReplaceOptions() { IsUpsert = false, BypassDocumentValidation = true, };
        protected override async Task<bool> ReplaceDocument(T t, RepoSaveArgs args = null)
        {
            if (string.IsNullOrEmpty(t.VersionTag))
            {
                t.VersionTag = HashUtils.NewGuid();
                InsertOneOptions insertOptions = new InsertOneOptions()
                {
                    BypassDocumentValidation = true,
                };
                await _collection.InsertOneAsync(t, insertOptions);
                return true;
            }


            string oldUpdateTag = t.VersionTag;
            string newUpdateTag = HashUtils.NewGuid();
            t.VersionTag = newUpdateTag;

            IClientSessionHandle session = null;

            if (args != null)
            {
                session = args.Args as IClientSessionHandle;
            }

            ReplaceOneResult result;
            if (session != null)
            {
                result = await _collection.ReplaceOneAsync(session, x => x.Id == t.Id && x.VersionTag == oldUpdateTag, t, _casReplaceOptions);
            }
            else
            {
                result = await _collection.ReplaceOneAsync(x => x.Id == t.Id && x.VersionTag == oldUpdateTag, t, _casReplaceOptions);
            }

            // If it didn't find a match, it means the version (VersionTag) changed out from under you
            if (result.MatchedCount == 0)
            {
                // Revert the tag changes so the local object isn't corrupted with a fake tag
                t.VersionTag = oldUpdateTag;
                throw new Exception("Optimistic concurrency violation: The document has been modified by another process." + oldUpdateTag);
            }

            return result.MatchedCount == 1;
        }

        protected override int GetMaxUpdateAttempts()
        {
            return 7;
        }

        class StubUpdateData : IVersionedData
        {
            public DateTime CreateTime { get; set; }
            public string VersionTag { get; set; }
        }


        string updateMemberName = null;
        protected override Dictionary<string, object> UpdateFieldNameUpdates(Dictionary<string, object> fieldNameUpdates)
        {
            if (string.IsNullOrEmpty(updateMemberName))
            {
                StubUpdateData updateData = new StubUpdateData();
                updateMemberName = nameof(updateData.VersionTag);
            }

            if (!fieldNameUpdates.ContainsKey(updateMemberName))
            {
                fieldNameUpdates[updateMemberName] = HashUtils.NewGuid().ToString();
            }

            return fieldNameUpdates;
        }
    }
}


