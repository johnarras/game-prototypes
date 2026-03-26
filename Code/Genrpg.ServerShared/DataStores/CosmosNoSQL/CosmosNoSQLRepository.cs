using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.DataStores.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.ServerShared.DataStores.CosmosNoSQL
{
    public class CosmosNoSQLRepository : IRepository
    {
        static readonly string PartitionKeyPath = "/" + (nameof(IPartitionedData.pk));
        private ILogService _logService = null;
        private ITextSerializer _serializer = null;
        private Database _database = null;
        private Microsoft.Azure.Cosmos.Container _container = null;

        private CosmosClient _client = null;

        private ItemRequestOptions _requestOptions = null;



        private JsonSerializerOptions _serializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };


        private CancellationToken _token;

        public async Task Init(InitRepoArgs args,
            CosmosClient client,
            ILogService logService,
            IAnalyticsService analyticsService,
            ITextSerializer serializer,
            CancellationToken token)
        {
            _token = token;
            string databaseName = DbUtils.GetDbName(args.Category.ToString(), args.Env);
            _logService = logService;
            _serializer = serializer;
            _client = client;

            _requestOptions = new ItemRequestOptions()
            {
                PriorityLevel = PriorityLevel.High,
                EnableContentResponseOnWrite = false,
                ConsistencyLevel = ConsistencyLevel.Session,
                IndexingDirective = IndexingDirective.Exclude,
                // If there's a cache set it here
                //DedicatedGatewayRequestOptions = new DedicatedGatewayRequestOptions() { BypassIntegratedCache = false, MaxIntegratedCacheStaleness = TimeSpan.FromMinutes(5) }
            };


            _database = await _client.CreateDatabaseIfNotExistsAsync(databaseName, requestOptions: _requestOptions, cancellationToken: token);

            ContainerProperties props = new ContainerProperties(args.Category.ToString(), PartitionKeyPath);
            props.IndexingPolicy.IndexingMode = IndexingMode.None;
            props.IndexingPolicy.Automatic = false;

            _container = await _database.CreateContainerIfNotExistsAsync(props);
        }


        protected string GetDocIdSuffix(Type t)
        {

            if (_docSuffixes.TryGetValue(t, out string suffix))
            {
                return suffix;
            }

            suffix = NoSqlUtils.GetDocIdSuffix(t);

            _docSuffixes.TryAdd(t, suffix);

            return suffix;
        }

        protected ConcurrentDictionary<Type, string> _docSuffixes = new ConcurrentDictionary<Type, string>();
        protected string GetDocId(string id, Type t)
        {
            string suffix = GetDocIdSuffix(t);

            if (!id.Contains(suffix))
            {
                return id + suffix;
            }
            return id;
        }

        public async Task<T> Load<T>(string id) where T : class, IStringId
        {
            ItemResponse<T> response = null;
            try
            {
                response = await _container.ReadItemAsync<T>(GetDocId(id, typeof(T)), new PartitionKey(id), _requestOptions, _token);
                return response.Resource;
            }
            catch (Microsoft.Azure.Cosmos.CosmosException ce)
            {

                if (ce.Message.IndexOf("NotFound") >= 0)
                {
                    return null;
                }
                _logService.Exception(ce, "CosmosNoSQL.Load");
            }
            catch (Exception e)
            {
                _logService.Exception(e, "CosmosNoSQL.Load");
            }
            return null;
        }


        public async Task<bool> Save<T>(T obj, RepoSaveArgs args = null) where T : IStringId
        {
            if (obj is IPartitionedData pd)
            {
                string suffix = NoSqlUtils.GetDocIdSuffix(obj.GetType());
                if (obj.Id.IndexOf(suffix) < 0)
                {
                    obj.Id += suffix;
                }
                ArrayPoolBufferWriter<byte> bufferWriter = _serializer.GetBuffer();

                using (Utf8JsonWriter jsonWriter = new Utf8JsonWriter(bufferWriter))
                {
                    JsonSerializer.Serialize(jsonWriter, obj, obj.GetType(), _serializerOptions);
                }

                Stream stream = bufferWriter.WrittenMemory.AsStream();

                ItemRequestOptions saveOptions = new ItemRequestOptions()
                {
                    PriorityLevel = _requestOptions.PriorityLevel,
                    IndexingDirective = _requestOptions.IndexingDirective,
                    EnableContentResponseOnWrite = _requestOptions.EnableContentResponseOnWrite,
                    ConsistencyLevel = _requestOptions.ConsistencyLevel,
                    IfMatchEtag = pd._etag,
                };

                PartitionKey pk = new PartitionKey(pd.pk);

                ResponseMessage message = await _container.ReplaceItemStreamAsync(stream, pd.Id, pk, saveOptions, _token);

                if (message.StatusCode == HttpStatusCode.NotFound)
                {
                    message = await _container.CreateItemStreamAsync(stream, pk, saveOptions, _token);
                }


                bufferWriter?.Dispose();
                stream?.Dispose();

                return message.StatusCode == HttpStatusCode.Created;

            }
            return false;
        }

        public async Task<bool> Delete<T>(T obj) where T : class, IStringId
        {
            string pk = null;
            try
            {
                if (obj is IPartitionedData pd)
                {
                    pk = pd.pk;
                    await _container.DeleteItemAsync<T>(GetDocId(obj.Id, obj.GetType()), new PartitionKey(obj.Id), _requestOptions, _token);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logService.Exception(e, "CosmosNoSQl.Delete");
            }
            return false;
        }

        public async Task<bool> TransactionSave<T>(List<T> items, RepoSaveArgs args = null) where T : IPartitionedData
        {
            List<ArrayPoolBufferWriter<byte>> bufferWriters = new List<ArrayPoolBufferWriter<byte>>();
            List<Stream> streams = new List<Stream>();
            try
            {
                foreach (T item in items)
                {
                    item.Id = GetDocId(item.Id, item.GetType());
                }

                // 1. Group by Partition Key (Batch only works per Partition)
                IEnumerable<IGrouping<string, T>> groups = items.GroupBy(x => x.pk);

                foreach (IGrouping<string, T> group in groups)
                {

                    if (group.Count() == 1)
                    {
                        return await Save(group.First(), null);
                    }


                    // 2. Initialize the batch for this specific Partition Key
                    PartitionKey partitionKey = new PartitionKey(group.Key);
                    TransactionalBatch batch = _container.CreateTransactionalBatch(partitionKey);


                    foreach (T item in group)
                    {
                        ArrayPoolBufferWriter<byte> bufferWriter = _serializer.GetBuffer();
                        bufferWriters.Add(bufferWriter);

                        using (Utf8JsonWriter jsonWriter = new Utf8JsonWriter(bufferWriter))
                        {
                            JsonSerializer.Serialize(jsonWriter, item, item.GetType(), _serializerOptions);
                        }

                        Stream stream = bufferWriter.WrittenMemory.AsStream();
                        streams.Add(stream);

                        TransactionalBatchItemRequestOptions options = new TransactionalBatchItemRequestOptions
                        {
                            IfMatchEtag = item._etag,
                            IndexingDirective = IndexingDirective.Exclude,
                            EnableContentResponseOnWrite = false,
                            PriorityLevel = PriorityLevel.High,
                        };
                        batch.UpsertItemStream(stream, options);
                    }

                    // 4. Execute the transaction
                    using TransactionalBatchResponse response = await batch.ExecuteAsync();


                    if (!response.IsSuccessStatusCode)
                    {
                        // 5. Handle Concurrency Failures
                        // If any item fails the ETag check, the whole batch fails (Status 412)
                        _logService.Error($"Batch failed with status: {response.StatusCode}");

                        // You can find which specific item failed:
                        for (int i = 0; i < response.Count; i++)
                        {
                            TransactionalBatchOperationResult<IPartitionedData> result = response.GetOperationResultAtIndex<IPartitionedData>(i);
                            if (result.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                            {
                                _logService.Error($"Item {i} (ID: {group.ElementAt(i).Id}) had a concurrency conflict.");
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "Cosmos.TransactionSave");
                return false;
            }
            finally
            {

                foreach (Stream stream in streams)
                {
                    stream.Dispose();
                }
                streams.Clear();

                foreach (ArrayPoolBufferWriter<byte> writer in bufferWriters)
                {
                    writer.Dispose();
                }
                bufferWriters.Clear();
            }
        }
    }
}
