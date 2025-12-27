using CommunityToolkit.HighPerformance.Buffers;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.DataStores;
using Genrpg.ServerShared.DataStores.CosmosNoSQL;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Website.Messages.Error;
using MongoDB.Driver;

namespace Genrpg.RequestServer.Core
{

    public class WebContext : ServerGameState, IDisposable
    {
        public GameAccount acct { get; set; }

        public CoreUserData user { get; set; }

        public MyRandom rand { get; set; } = new MyRandom();

        protected WebResponseList Responses { get; set; } = new WebResponseList();

        protected IFullRepositoryService _repoService = null;

        protected IBinarySerializer _binarySerializer = null;

        public WebContext(IServerConfig config) : base(config)
        {

        }

        public void AddResponse(IWebResponse response)
        {
            Responses.AddResponse(response);
        }

        public void AddFront(IWebResponse response)
        {
            Responses.AddFront(response);
        }

        public List<IWebResponse> GetResponseList()
        {
            return Responses.GetResponses();
        }

        public IFullRepositoryService GetRepositoryService()
        {
            return _repoService;
        }

        public void ClearResponses()
        {
            Responses.Clear();
        }

        public void AddResponseRange(List<IWebResponse> responses)
        {
            Responses.AddRange(responses);
        }

        public WebContext(IServerConfig config, IServiceLocator locIn, IFullRepositoryService repoService, IBinarySerializer binarySerializer) : base(config)
        {
            _loc = locIn;
            rand = new MyRandom();
            _repoService = repoService;
            _binarySerializer = binarySerializer;
        }

        public async Task<GameAccount> LoadUser(string userId)
        {
            if (acct == null)
            {
                acct = await _repoService.Load<GameAccount>(userId);
            }
            return acct;
        }

        protected class UnitDataSnapShotMustDispose : IDisposable
        {

            public ReadOnlySpan<byte> WrittenSpan
            {
                get
                {
                    return _bufferWriterMustDispose.WrittenSpan;
                }
            }

            private ArrayPoolBufferWriter<byte> _bufferWriterMustDispose { get; init; }

            public IUnitData UnitData => _unitData;
            private IUnitData _unitData { get; init; }

            public void Dispose()
            {
                _bufferWriterMustDispose.Dispose();
            }

            public UnitDataSnapShotMustDispose(ArrayPoolBufferWriter<byte> bufferWriterMustDispose, IUnitData unitData)
            {
                _bufferWriterMustDispose = bufferWriterMustDispose;
                _unitData = unitData;
            }
        }

        protected Dictionary<Type, UnitDataSnapShotMustDispose> _unitData = new Dictionary<Type, UnitDataSnapShotMustDispose>();

        public List<IUnitData> AllData() { return _unitData.Values.Select(x => x.UnitData).ToList(); }

        public void Set(IUnitData doc)
        {
            string id = doc.Id;
            if (doc is IId iid)
            {
                id = iid.IdKey.ToString();
            }

            if (_unitData.TryGetValue(doc.GetType(), out UnitDataSnapShotMustDispose currSnapshot))
            {
                currSnapshot.Dispose();
            }

            ArrayPoolBufferWriter<byte> buffer = _binarySerializer.GetBuffer();
            _binarySerializer.BinarySerialize(doc, buffer);
            _unitData[doc.GetType()] = new UnitDataSnapShotMustDispose(buffer, doc);
        }

        public void Remove<T>(string docId) where T : class, IUnitData, new()
        {
            if (_unitData.TryGetValue(typeof(T), out UnitDataSnapShotMustDispose snapshot))
            {
                _unitData.Remove(typeof(T));
                snapshot.Dispose();
            }
        }

        public async Task<T> GetAsync<T>(string id = null) where T : class, IUnitData, new()
        {
            if (string.IsNullOrEmpty(id))
            {
                if (typeof(IUserData).IsAssignableFrom(typeof(T)))
                {
                    id = acct.Id;
                }
                else if (!string.IsNullOrEmpty(acct.CurrCharId))
                {
                    id = acct.CurrCharId;
                }
                else
                {
                    return default;
                }
            }

            if (_unitData.TryGetValue(typeof(T), out UnitDataSnapShotMustDispose snapshot))
            {
                return (T)snapshot.UnitData;
            }

            T item = await _repoService.Load<T>(id);

            if (item == null)
            {
                item = new T() { Id = id };
            }
            Set(item);
            return item;
        }

        public void ShowError(string error)
        {
            AddResponse(new ErrorResponse() { Error = error });
        }

        private bool _didSave = false;

        /// <summary>
        /// Saves all player data that's been modified. Only call this once because the objects in the DB
        /// can only be written to once.
        /// </summary>
        /// <returns></returns>
        public async Task SaveAllOneTime()
        {

            List<Task> saveTasks = new List<Task>();


            List<IUniquePersonalUserData> partitionedData = new List<IUniquePersonalUserData>();
            List<IUnitData> otherData = new List<IUnitData>();

            ArrayPoolBufferWriter<byte> tempBufferMustDispose = _binarySerializer.GetBuffer();
            foreach (UnitDataSnapShotMustDispose snapshot in _unitData.Values)
            {
                _binarySerializer.BinarySerialize(snapshot.UnitData, tempBufferMustDispose);

                if (!MemoryExtensions.SequenceEqual(snapshot.WrittenSpan, tempBufferMustDispose.WrittenSpan))
                {
                    if (snapshot.UnitData is IUniquePersonalUserData pd)
                    {
                        partitionedData.Add(pd);
                    }
                    else
                    {
                        saveTasks.Add(_repoService.Save(snapshot.UnitData));
                    }
                }

                snapshot.Dispose();
            }

            // Now dispose all. Cannot call this again.
            tempBufferMustDispose.Dispose();

            AddCosmosSaveTask(saveTasks, partitionedData);

            await Task.WhenAll(saveTasks);

            _didSave = true;
        }

        public void Dispose()
        {
            if (_didSave)
            {
                return;
            }

            foreach (UnitDataSnapShotMustDispose snapshot in _unitData.Values)
            {
                snapshot.Dispose();
            }
        }

        protected void AddCosmosSaveTask(List<Task> saveTasks, List<IUniquePersonalUserData> coreData)
        {
            if (coreData.Count < 1)
            {
                return;
            }

            FullRepositoryService fullService = _repoService as FullRepositoryService;

            CosmosNoSQLRepository cosmosRepo = fullService.FindRepo(coreData[0].GetType()) as CosmosNoSQLRepository;

            saveTasks.Add(cosmosRepo.TransactionSave(coreData));
        }
    }
}


