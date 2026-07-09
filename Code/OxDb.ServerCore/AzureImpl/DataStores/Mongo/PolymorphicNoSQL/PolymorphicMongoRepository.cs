using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using OxDb.ServerCore.AzureImpl.DataStores.Entities;
using OxDb.ServerCore.AzureImpl.DataStores.Mongo.Interfaces;
using OxDb.SharedCore.DataStores.Constants;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.DataStores.Entities;
using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.DataStores.Utils;
using System.Linq.Expressions;
using System.Reflection;

namespace OxDb.ServerCore.AzureImpl.DataStores.Mongo.PolymorphicNoSQL
{
    public class PolymorphicMongoRepository : ISearchRepository, IMongoInitRepository
    {

        private ILogService _logService = null;

        private MongoClient _client = null;
        private IMongoDatabase _database = null;
        private IMongoCollection<BsonDocument> _collection = null;
        private ITextSerializer _serializer = null;
        private IReflectionService _reflectionService = null;

        private CancellationToken _token;

        private static object _initializeLock = new object();
        private static bool _didInitializeMappings = false;

        private const string CollectionName = "alldata";

        private string _databaseName = null;

        public async Task Init(InitRepoArgs args,
            MongoClient client,
            ILogService logService,
            ITextSerializer serializer,
            IReflectionService reflectionService,
            CancellationToken token)
        {
            _token = token;
            string databaseName = DbUtils.GetDbName(args.ProductName, args.Category.ToString(), args.Env);
            _databaseName = databaseName;
            _logService = logService;
            _serializer = serializer;
            _client = client;
            _database = _client.GetDatabase(databaseName);
            _collection = _database.GetCollection<BsonDocument>(CollectionName);
            if (!_didInitializeMappings)
            {
                lock (_initializeLock)
                {
                    if (!_didInitializeMappings)
                    {
                        List<Type> types = reflectionService.GetTypesWithAttribute(typeof(DataGroup));
                        foreach (Type type in types)
                        {
                            DataGroup group = type.GetCustomAttribute<DataGroup>(true);
                            if (group != null && group.RepoType == ERepoTypes.Polymorphic)
                            {
                                if (!BsonClassMap.IsClassMapRegistered(type))
                                {
                                    BsonClassMap classMap = new BsonClassMap(type);
                                    classMap.AutoMap();
                                    classMap.SetDiscriminator(StrUtils.NormalizeTypeName(type));
                                    BsonClassMap.RegisterClassMap(classMap);
                                }
                            }
                        }
                        _didInitializeMappings = true;
                    }
                }

                CreateIndexData indexData = new CreateIndexData(typeof(BsonDocument));
                indexData.Configs.Add(new IndexConfig() { MemberName = DatabaseConstants.TypeDiscriminatorMemberName, Ascending = true, CompoundContinue = false });
                await CreateIndex(indexData);
            }
        }

        public IMongoCollection<BsonDocument> GetSettingsCollection()
        {
            if (_databaseName.IndexOf(EDataCategories.Settings.ToString().ToLower()) < 0)
            {
                return null;
            }

            return _collection;
        }


        public string GetFullDocId(Type t, string id)
        {
            string typeNameLower = StrUtils.NormalizeTypeName(t);

            return GetFullDocId(typeNameLower, id);

        }

        public string GetFullDocId(string typeName, string id)
        {

            string typeNameLower = typeName.ToLower();

            string finalId = id;
            if (id.IndexOf(typeNameLower) < 0)
            {
                finalId = typeNameLower + id;
            }

            return finalId;
        }

        public async Task<T> Load<T>(string id) where T : class, IStringId
        {
            string fullId = GetFullDocId(typeof(T), id);

            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", fullId);

            try
            {
                BsonDocument doc = await _collection.Find(filter).FirstOrDefaultAsync();

                if (doc != null)
                {
                    T t = BsonSerializer.Deserialize<T>(doc);
                    t.Id = id;
                    return t;
                }
            }
            catch (Exception e)
            {
                _logService.Exception(e, "PolymorphicMongo.Load");
            }
            return default;
        }

        private ReplaceOptions _saveOptions = new ReplaceOptions() { IsUpsert = true, BypassDocumentValidation = true, };
        public async Task<bool> Save<T>(T obj, RepoSaveArgs args = null) where T : IStringId
        {

            try
            {
                string fullId = GetFullDocId(obj.GetType(), obj.Id);

                BsonDocument doc = obj.ToBsonDocument();
                doc["_id"] = fullId;

                ReplaceOneResult replaceResult = await _collection.ReplaceOneAsync(
                    filter: Builders<BsonDocument>.Filter.Eq("_id", fullId),
                    replacement: doc,
                    options: _saveOptions
                    );

                if (replaceResult.ModifiedCount < 1 && string.IsNullOrEmpty(replaceResult.UpsertedId?.AsString ?? null))
                {
                    string errorString = "Failed to upsert Polymorphic Document " + typeof(T).Name + " Id: " + fullId;
                    _logService.Error(errorString);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "PolymorphicMongo.Save");
                return false;
            }

            return true;
        }


        public async Task<bool> Delete<T>(T obj) where T : class, IStringId
        {
            string fullId = GetFullDocId(obj.GetType(), obj.Id);
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", fullId);

            try
            {
                DeleteResult result = await _collection.DeleteOneAsync(filter);
                if (result.DeletedCount < 1)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "PolymorphicMongo.Delete");
                return false;
            }
            return true;
        }

        public async Task<List<T>> LoadAll<T>() where T : IStringId
        {
            IMongoCollection<BsonDocument> collection = _database.GetCollection<BsonDocument>(CollectionName);

            IAsyncCursor<BsonDocument> cursor = await collection.FindAsync<BsonDocument>(x => true);

            List<BsonDocument> docs = await cursor.ToListAsync<BsonDocument>();

            List<T> results = new List<T>();

            foreach (BsonDocument doc in docs)
            {
                if (doc.Contains(DatabaseConstants.TypeDiscriminatorMemberName))
                {
                    BsonValue discriminator = doc[DatabaseConstants.TypeDiscriminatorMemberName];

                    try
                    {
                        // Look up the actual type via the driver's mapped cache
                        Type targetType = BsonSerializer.LookupActualType(typeof(T), discriminator);

                        if (targetType != null)
                        {
                            T typedObject = (T)BsonSerializer.Deserialize(doc, targetType);
                            if (typedObject != null)
                            {
                                results.Add(typedObject);
                            }
                        }
                    }
                    catch (Exception ee)
                    {
                        _logService.Exception(ee, "Polymorphic.LoadAll");
                    }
                }
            }
            return results;
        }

        public async Task<List<T>> Search<T>(object funcObj, int quantity, int skip) where T : class, ISearchableItem
        {
            try
            {

                string typeName = StrUtils.NormalizeTypeName<T>();
                if (funcObj is not Expression<Func<T, bool>> tFilter)
                {
                    return new List<T>();
                }

                IMongoCollection<T> collection = _database.GetCollection<T>(CollectionName);

                FilterDefinition<T> typeFilter = Builders<T>.Filter.Eq(DatabaseConstants.TypeDiscriminatorMemberName, typeName);

                FilterDefinition<T> fullFilter = Builders<T>.Filter.And(typeFilter, tFilter);

                IFindFluent<T, T> query = collection.Find<T>(fullFilter);

                if (skip > 0)
                {
                    query = query.Skip(skip);
                }
                if (quantity > 0)
                {
                    query = query.Limit(quantity);
                }

                List<T> retval = await query.ToListAsync();

                foreach (T t in retval)
                {
                    t.Id = t.Id.Replace(typeName, "");
                }
                return retval;
            }
            catch (Exception ee)
            {
                _logService.Exception(ee, "SingleCollectionRepo.Search");
            }
            return new List<T>();
        }



        public async Task CreateIndexes(CreateIndexData data)
        {

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

            Type thisType = typeof(BsonDocument);
            IndexKeysDefinitionBuilder<BsonDocument> indexBuilder = Builders<BsonDocument>.IndexKeys;

            List<IndexKeysDefinition<BsonDocument>> allKeys = new List<IndexKeysDefinition<BsonDocument>>();
            foreach (IndexConfig config in data.Configs)
            {
                CreateIndexOptions<BsonDocument> options = new CreateIndexOptions<BsonDocument>()
                {
                    Unique = config.Unique,
                };

                MemberInfo mem = thisType.GetMembers(BindingFlags.Public).FirstOrDefault(x => x.Name == config.MemberName);
                if (mem == null && config.MemberName != DatabaseConstants.TypeDiscriminatorMemberName)
                {
                    continue;
                }
                StringFieldDefinition<BsonDocument> fieldDef = new StringFieldDefinition<BsonDocument>(config.MemberName);

                allKeys.Add(config.Ascending ?
                    indexBuilder.Ascending(fieldDef) :
                    indexBuilder.Descending(fieldDef));

                if (config.CompoundContinue)
                {
                    continue;
                }
                else
                {
                    IndexKeysDefinition<BsonDocument> indexDef = Builders<BsonDocument>.IndexKeys.Combine(allKeys.ToArray());
                    CreateIndexModel<BsonDocument> indexModel = new CreateIndexModel<BsonDocument>(indexDef, options);

                    await _collection.Indexes.CreateOneAsync(indexModel);

                    allKeys.Clear();
                }
            }
        }

    }
}


