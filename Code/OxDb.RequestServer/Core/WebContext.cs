using CommunityToolkit.HighPerformance.Buffers;
using MongoDB.Driver;
using OxDb.RequestServer.Core.Services;
using OxDb.ServerCore.Config;
using OxDb.ServerCore.Core;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedCore.Website.Responses.Errors;
using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;

namespace OxDb.RequestServer.Core
{

    public class WebContext : ServerGameState, IDisposable, IUnitDataLookup, IWebContext, IRandomContainer
    {

        public string GameUserId => _gameUserId;
        private string _gameUserId { get; set; }

        private long _existingData { get; set; }
        public long ExistingData => _existingData;

        public IRandom Rand { get; set; } = new MyRandom();

        protected WebResponseList Responses { get; set; } = new WebResponseList();

        protected IRepositoryService _repoService = null;

        protected IBinarySerializer _binarySerializer = null;

        protected IPartitionedDataSaveService _partitionedSaveService = null;

        public WebContext(IServerConfig config, ILogService logService) : base(config, logService)
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

        public void ClearResponses()
        {
            Responses.Clear();
        }

        public void AddResponseRange(List<IWebResponse> responses)
        {
            Responses.AddRange(responses);
        }

        public WebContext(IServerConfig config, ILogService logService, IServiceLocator locIn, IRepositoryService repoService, IBinarySerializer binarySerializer,
            IPartitionedDataSaveService partitionedSaveService) : base(config, logService)
        {
            _loc = locIn;
            Rand = new MyRandom();
            _repoService = repoService;
            _binarySerializer = binarySerializer;
            _partitionedSaveService = partitionedSaveService;
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

        public void SetGameUserId(string gameUserId)
        {
            _gameUserId = gameUserId;
        }

        public void SetExistingData(string existingDataString)
        {
            if (Int64.TryParse(existingDataString, out long value))
            {
                _existingData = value;
            }
            else
            {
                throw new Exception("Invalid Existing Data");
            }
        }

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

        public async Task<T> GetAsync<T>() where T : class, IUnitData, new()
        {

            if (string.IsNullOrEmpty(_gameUserId))
            {
                throw new Exception("GameAccount was not set!");
            }

            if (_unitData.TryGetValue(typeof(T), out UnitDataSnapShotMustDispose snapshot))
            {
                return (T)snapshot.UnitData;
            }

            T item = await _repoService.Load<T>(_gameUserId);

            if (item == null)
            {
                T newItem = new T() { Id = _gameUserId };

                if (newItem is IUniquePersonalUserData personalData)
                {
                    if (FlagUtils.HasBitIndex(_existingData, personalData.GetOffsetBit()))
                    {
                        throw new Exception($"Existing document of type {personalData.GetType().Name} for UserId {_gameUserId} failed to load");
                    }
                }
                item = newItem;
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
        public async Task<bool> SaveAllOneTime()
        {
            if (_didSave)
            {
                return false;
            }

            List<Task<bool>> saveTasks = new List<Task<bool>>();


            List<IUniquePersonalUserData> partitionedData = new List<IUniquePersonalUserData>();
            List<IUnitData> otherData = new List<IUnitData>();

            ArrayPoolBufferWriter<byte> tempBufferMustDispose = _binarySerializer.GetBuffer();
            foreach (UnitDataSnapShotMustDispose snapshot in _unitData.Values)
            {
                _binarySerializer.BinarySerialize(snapshot.UnitData, tempBufferMustDispose);

                if (snapshot.UnitData is IUniquePersonalUserData pd)
                {
                    if (string.IsNullOrEmpty(pd._etag) || !MemoryExtensions.SequenceEqual(snapshot.WrittenSpan, tempBufferMustDispose.WrittenSpan))
                    {
                        partitionedData.Add(pd);
                    }
                }
                else if (!MemoryExtensions.SequenceEqual(snapshot.WrittenSpan, tempBufferMustDispose.WrittenSpan))
                {
                    saveTasks.Add(_repoService.Save(snapshot.UnitData));
                }

                snapshot.Dispose();
            }

            // Now dispose all. Cannot call this again.
            tempBufferMustDispose.Dispose();

            saveTasks.Add(_partitionedSaveService.SavePartitionedList(partitionedData, _repoService));

            bool[] results = await Task.WhenAll(saveTasks);

            if (results.Any(x => x == false))
            {
                return false;
            }

            _didSave = true;
            return true;
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

    }
}


