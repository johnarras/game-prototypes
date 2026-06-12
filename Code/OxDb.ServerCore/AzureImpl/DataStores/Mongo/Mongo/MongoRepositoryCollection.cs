using MongoDB.Bson;
using MongoDB.Driver;
using OxDb.ServerCore.AzureImpl.DataStores.Mongo.Interfaces;
using OxDb.SharedCore.DataStores.Entities;
using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace OxDb.ServerCore.AzureImpl.DataStores.Mongo.Mongo
{
    public class MongoRepositoryCollection<T> : ITypedNoSQLCollection<T> where T : class, ISearchableItem, IStringId
    {
        protected IMongoCollection<T> _collection = null;
        protected ILogService _logService = null;
        public MongoRepositoryCollection(IMongoDatabase mongoDatabase, ILogService logService)
        {
            _logService = logService;
            _collection = mongoDatabase.GetCollection<T>(GetCollectionName());

        }

        protected async Task<List<string>> GetIndexedFieldNames()
        {
            List<string> retval = new List<string>();
            IAsyncCursor<BsonDocument> indexCursor = await _collection.Indexes.ListAsync();
            List<BsonDocument> indexDocs = await indexCursor.ToListAsync();
            retval = indexDocs
                .SelectMany(doc => doc["key"].AsBsonDocument.Names)
                .Where(name => name != "_id")
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return retval;
        }

        private string GetCollectionName()
        {
            return (typeof(T).Name + "doc").ToLower();
        }

        public async Task<bool> DeleteAll(object listObj)
        {
            Expression<Func<T, bool>> func = (Expression<Func<T, bool>>)listObj;


            if (func == null)
            {
                return false;
            }

            DeleteResult deleteResult = await _collection.DeleteManyAsync<T>(func);
            return deleteResult.DeletedCount >= 0;
        }


        public async Task<bool> Delete(object obj)
        {
            T t = (T)obj;

            if (t == null)
            {
                return false;
            }

            try
            {
                DeleteResult result = await _collection.DeleteOneAsync(x => x.Id == t.Id);
                if (result.DeletedCount < 1)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "Mongo.Delete");
                return false;
            }
            return true;
        }

        public async Task<object> Load(string id)
        {
            try
            {
                IAsyncCursor<T> cursor = await _collection.FindAsync(x => x.Id == id);
                return await cursor.FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "Mongo.Load");
            }
            return null;
        }

        public async Task<bool> Save(object obj, RepoSaveArgs args = null)
        {
            return await InnerSave(obj, args);
        }

        public async Task<bool> TransactionSave(object obj, RepoSaveArgs args = null)
        {
            return await InnerSave(obj, args);
        }


        protected async Task<bool> InnerSave(object obj, RepoSaveArgs args = null)
        {
            T t = (T)obj;

            if (t == null)
            {
                return false;
            }

            try
            {
                if (string.IsNullOrEmpty(t.Id))
                {
                    throw new Exception("Missing Id on save");
                }

                return await ReplaceDocument(t, args);

            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "Mongo.Save");
                return false;
            }
            return true;
        }

        private ReplaceOptions _upsertSaveOptions = new ReplaceOptions() { IsUpsert = true, BypassDocumentValidation = true, };
        virtual protected async Task<bool> ReplaceDocument(T t, RepoSaveArgs args = null)
        {
            IClientSessionHandle session = null;

            if (args != null)
            {
                session = args.Args as IClientSessionHandle;
            }

            ReplaceOneResult result;
            if (session != null)
            {
                result = await _collection.ReplaceOneAsync(session, w => w.Id == t.Id, t, _upsertSaveOptions);
            }
            else
            {
                result = await _collection.ReplaceOneAsync(w => w.Id == t.Id, t, _upsertSaveOptions);
            }

            if (result.ModifiedCount < 1 && string.IsNullOrEmpty(result.UpsertedId?.AsString ?? null))
            {
                string errorString = "Failed to upsert Document " + typeof(T).Name + " Id: " + t.Id;
                _logService.Error(errorString);
                return false;
            }

            return result.ModifiedCount == 1;
        }

        public async Task<List<T>> Search(object funcObj, int quantity = 1000, int skip = 0)
        {
            Expression<Func<T, bool>> func = (Expression<Func<T, bool>>)funcObj;

            if (func == null)
            {
                return new List<T>();
            }

            try
            {
                FindOptions<T, T> options = new FindOptions<T, T>();
                if (skip > 0)
                {
                    options.Skip = skip;
                }
                if (quantity > 0)
                {
                    options.Limit = quantity;
                }
                IAsyncCursor<T> cursor = await _collection.FindAsync(func, options);
                return await cursor.ToListAsync();
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "Mongo.Search");
            }
            return new List<T>();
        }


        public async Task CreateIndex(CreateIndexData data)
        {
            List<string> currentIndexes = await GetIndexedFieldNames();

            List<string> newIndexes = data.Configs.Select(x => x.MemberName).OrderBy(x => x).ToList();

            if (currentIndexes.Count == newIndexes.Count)
            {
                bool allSame = true;
                for (int i = 0; i < currentIndexes.Count; i++)
                {
                    if (currentIndexes[i] != newIndexes[i])
                    {
                        allSame = false;
                        break;
                    }
                }
                if (allSame)
                {
                    return;
                }
            }

            List<IndexConfig> orderedConfigs = data.Configs.OrderBy(x => x.MemberName).ToList();

            string totalIndex = "";
            foreach (IndexConfig config in orderedConfigs)
            {
                totalIndex += config.MemberName;
            }

            Type thisType = typeof(T);
            IndexKeysDefinitionBuilder<T> indexBuilder = Builders<T>.IndexKeys;

            List<IndexKeysDefinition<T>> allKeys = new List<IndexKeysDefinition<T>>();
            foreach (IndexConfig config in data.Configs)
            {
                CreateIndexOptions<T> options = new CreateIndexOptions<T>()
                {
                    Unique = config.Unique,
                };

                MemberInfo mem = thisType.GetMembers(BindingFlags.Public).FirstOrDefault(x => x.Name == config.MemberName);
                if (mem == null)
                {
                    continue;
                }
                StringFieldDefinition<T> fieldDef = new StringFieldDefinition<T>(config.MemberName);

                allKeys.Add(config.Ascending ?
                    indexBuilder.Ascending(fieldDef) :
                    indexBuilder.Descending(fieldDef));

                if (config.CompoundContinue)
                {
                    continue;
                }
                else
                {
                    IndexKeysDefinition<T> indexDef = Builders<T>.IndexKeys.Combine(allKeys.ToArray());
                    CreateIndexModel<T> indexModel = new CreateIndexModel<T>(indexDef, options);

                    await _collection.Indexes.CreateOneAsync(indexModel);

                    allKeys.Clear();
                }
            }
        }

        public async Task<bool> SaveAll(object listObj)
        {
            List<T> items = listObj as List<T>;

            if (items == null)
            {
                return false;
            }
            if (items.Count < 1)
            {
                return true;
            }
            try
            {
                List<WriteModel<T>> models = new List<WriteModel<T>>();

                foreach (T item in items)
                {
                    ReplaceOneModel<T> replaceModel = new ReplaceOneModel<T>(new FilterDefinitionBuilder<T>().Where(x => x.Id == item.Id), item);
                    replaceModel.IsUpsert = true;
                    models.Add(replaceModel);
                }

                BulkWriteOptions options = new BulkWriteOptions()
                {
                    BypassDocumentValidation = true,
                    IsOrdered = false,
                };
                await _collection.BulkWriteAsync(models, options);

            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "MongoRepo.SaveAll");
                return false;
            }
            return true;
        }

        protected virtual int GetMaxUpdateAttempts()
        {
            return 1;
        }

        protected virtual Dictionary<string, object> UpdateFieldNameUpdates(Dictionary<string, object> fieldNameUpdates)
        {
            return fieldNameUpdates;
        }

        public virtual async Task<bool> UpdateDict(string docId, Dictionary<string, object> fieldNameUpdates)
        {

            fieldNameUpdates = UpdateFieldNameUpdates(fieldNameUpdates);

            int maxAttempts = GetMaxUpdateAttempts();

            Expression<Func<T, bool>> filter = x => x.Id == docId;

            UpdateDefinitionBuilder<T> builder = Builders<T>.Update;

            List<UpdateDefinition<T>> updates = new List<UpdateDefinition<T>>();

            foreach (string fieldName in fieldNameUpdates.Keys)
            {
                updates.Add(builder.Set(fieldName, fieldNameUpdates[fieldName]));
            }

            UpdateDefinition<T> finalUpdateDef = builder.Combine(updates);

            UpdateOptions options = new UpdateOptions() { BypassDocumentValidation = true, IsUpsert = true };

            for (int i = 0; i < maxAttempts; i++)
            {
                UpdateResult result = await _collection.UpdateOneAsync(filter, finalUpdateDef, options);

                if (result.UpsertedId == docId)
                {
                    return true;
                }

                await Task.Delay(100);
            }

            return false;
        }

        public async Task<bool> UpdateAction(string docId, object actionObj)
        {
            Action<T> action = actionObj as Action<T>;

            if (action == null)
            {
                return false;
            }

            for (int times = 0; times < GetMaxUpdateAttempts(); times++)
            {
                T doc = (T)await Load(docId);

                if (doc != null)
                {
                    action(doc);

                    if (await Save(doc))
                    {
                        return true;
                    }
                }

                await Task.Delay(250);

            }
            return false;
        }

        /// <summary>
        /// This exists to let us do atomic increments. It's very low level so not exposed in the general IRepositoryService
        /// and requires a few steps to get to it.
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<object> AtomicIncrement(string docId, string fieldName, long value)
        {

            FilterDefinition<T> filter = Builders<T>.Filter.Eq(doc => doc.Id, docId);

            UpdateDefinition<T> update = Builders<T>.Update.Inc(fieldName, value);

            FindOneAndUpdateOptions<T> options = new FindOneAndUpdateOptions<T>()
            {
                ReturnDocument = ReturnDocument.After,
                BypassDocumentValidation = true,
            };

            return await _collection.FindOneAndUpdateAsync(filter, update, options);

        }

        public async Task<object> AtomicAddBits(string docId, string fieldName, long addBits)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq(doc => doc.Id, docId);

            UpdateDefinition<T> update = Builders<T>.Update.BitwiseOr(fieldName, addBits);

            FindOneAndUpdateOptions<T> options = new FindOneAndUpdateOptions<T>()
            {
                ReturnDocument = ReturnDocument.After,
                BypassDocumentValidation = true,
            };

            return await _collection.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task<object> AtomicRemoveBits(string docId, string fieldName, long removeBits)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq(doc => doc.Id, docId);

            UpdateDefinition<T> update = Builders<T>.Update.BitwiseAnd(fieldName, ~removeBits);

            FindOneAndUpdateOptions<T> options = new FindOneAndUpdateOptions<T>()
            {
                ReturnDocument = ReturnDocument.After,
                BypassDocumentValidation = true,
            };

            return await _collection.FindOneAndUpdateAsync(filter, update, options);
        }
    }
}


