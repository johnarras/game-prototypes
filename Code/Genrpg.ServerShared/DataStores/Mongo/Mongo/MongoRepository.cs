using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.ServerShared.DataStores.Mongo.Interfaces;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.DataStores.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Utils;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.Mongo.Mongo
{
    public class MongoRepository : IFullRepository, IMongoInitRepository
    {
        private ILogService _logService = null;
        private IReflectionService _reflectionService = null;

        private MongoClient _client = null;
        private IMongoDatabase _database = null;
        private ConcurrentDictionary<Type, INoSQLCollection> _collections = new ConcurrentDictionary<Type, INoSQLCollection>();
        private CancellationToken _token;

        #region Core
        public async Task Init(InitRepoArgs args,
            MongoClient client,
            ILogService logService,
            IAnalyticsService analyticsService,
            ITextSerializer serializer,
            IReflectionService reflectionService,
            CancellationToken token)
        {
            _token = token;
            string databaseName = DbUtils.GetDbName(args.Category.ToString(), args.Env);
            _logService = logService;
            _client = client;
            _database = _client.GetDatabase(databaseName);
        }
        public MongoClient GetClient()
        {
            return _client;
        }

        public IMongoDatabase GetDatabase()
        {
            return _database;
        }

        /// <summary>
        /// This is a bit ugly, but the fact that Mongo requires a generic type
        /// to perform operations means it's either something like this,
        /// or the generic type of everything has to properly be passed
        /// around the codebase, but this leads to a lot of tedious 
        /// helper classes and methods just to be able to have
        /// a list of some base class/interface and still be
        /// able to do database operations through the dynamic type
        /// of the object.
        /// 
        /// This is public, but not in an interface to expose a few helper functions.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected INoSQLCollection GetCollection(Type t)
        {
            if (_collections.TryGetValue(t, out INoSQLCollection coll))
            {
                return coll;
            }


            // This uses reflection here to avoid having generic scaffolding classes
            // grow throughout the program
            Type baseCollectionType = t.GetInterface(nameof(IVersionedData)) != null ?
                typeof(VersionedMongoCollection<>) :
                typeof(MongoRepositoryCollection<>);
            Type genericType = baseCollectionType.MakeGenericType(t);
            coll = (INoSQLCollection)Activator.CreateInstance(genericType, new object[] { GetDatabase(), _logService });
            _collections[t] = coll;
            return coll;
        }
        #endregion


        public async Task<T> Load<T>(string id) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));
            return (T)await collection.Load(id);
        }

        public async Task<bool> Save<T>(T obj, RepoSaveArgs args = null) where T : IStringId
        {
            INoSQLCollection collection = GetCollection(obj.GetType());
            return await collection.Save(obj, args);
        }

        public async Task<bool> Delete<T>(T obj) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(obj.GetType());
            return await collection.Delete(obj);
        }

        public async Task<bool> DeleteAll<T>(Expression<Func<T, bool>> func) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));
            return await collection.DeleteAll(func);
        }

        public async Task<List<T>> Search<T>(object funcObj, int quantity, int skip) where T : class, ISearchableItem
        {

            if (funcObj is Expression<Func<T, bool>> func)

            {
                ITypedNoSQLCollection<T> collection = GetCollection(typeof(T)) as ITypedNoSQLCollection<T>;

                return await collection.Search(func, quantity, skip);
            }
            return new List<T>();
        }

        /// <summary>
        /// This requires a generic type so that we can put the index directly onto the correct collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configs"></param>
        /// <returns></returns>
        public async Task CreateIndexes(CreateIndexData data)
        {
            INoSQLCollection collection = GetCollection(data.TypeToIndex);
            if (collection != null)
            {
                await collection.CreateIndex(data);
            }
        }

        /// <summary>
        /// This requires a generic type so we can save all of them into one collection at once.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public async Task<bool> SaveAll<T>(List<T> items) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));
            return await collection.SaveAll(items);
        }

        public async Task<bool> TransactionSave<T>(List<T> list) where T : class, IStringId
        {
            if (true)
            {
                List<Task<bool>> saves = new List<Task<bool>>();

                foreach (T item in list)
                {
                    INoSQLCollection collection = GetCollection(item.GetType());
                    saves.Add(collection.Save(item));
                }

                bool[] results = await Task.WhenAll(saves).ConfigureAwait(false);

                return !results.Any(x => x == false);
            }
            else // Azure Cosmos does not all multicollection transactions.
            {
                //using (IClientSessionHandle session = await _client.StartSessionAsync())
                //{
                //    try
                //    {
                //        session.StartTransaction();

                //        List<Task<bool>> saves = new List<Task<bool>>();

                //        foreach (T item in list)
                //        {
                //            IMongoCollection collection = GetCollection(item.GetType());
                //            saves.Add(collection.TransactionSave(item, session));
                //        }

                //        bool[] successes = await Task.WhenAll(saves);
                //        if (successes.Any(x => x == false))
                //        {
                //            await session.AbortTransactionAsync();
                //            return false;
                //        }
                //        else
                //        {
                //            await session.CommitTransactionAsync();
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        _logService.Exception(e, "MongoRepository.TransactionSave");
                //        await session.AbortTransactionAsync();
                //        throw new Exception("Failed Transaction", e);
                //    }
                //    return true;
                //}
            }
        }

        public virtual async Task<bool> UpdateDict<T>(string docId, Dictionary<string, object> fieldNameUpdates) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));
            return await collection.UpdateDict(docId, fieldNameUpdates);
        }

        public virtual async Task<bool> UpdateAction<T>(string docId, Action<T> action) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));
            return await collection.UpdateAction(docId, action);
        }

        public async Task<T> AtomicIncrement<T>(string docId, string fieldName, long increment) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));
            return (T)await collection.AtomicIncrement(docId, fieldName, increment);
        }


        public async Task<T> AtomicAddBits<T>(string docId, string fieldName, long addBits) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));
            return (T)await collection.AtomicAddBits(docId, fieldName, addBits);
        }


        public async Task<T> AtomicRemoveBits<T>(string docId, string fieldName, long removeBits) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));
            return (T)await collection.AtomicRemoveBits(docId, fieldName, removeBits);
        }
    }
}


