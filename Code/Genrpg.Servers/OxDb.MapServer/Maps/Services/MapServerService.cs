using OxDb.MapServer.MainServer;
using OxDb.MapServer.Maps.Constants;
using OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.ServerCore.Constants;
using OxDb.ServerCore.MainServer;
using OxDb.ServerGame.Maps;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.Networking.Constants;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.MapServer.Maps.Services
{
    public class MapServerService : IMapServerService
    {
        private IMapDataService _mapDataService = null;
        private ICloudCommsService _cloudCommsService = null;
        private ILogService _logService = null;

        private ConcurrentDictionary<string, MapInstance> _instances = new ConcurrentDictionary<string, MapInstance>();

        private string _mapServerName;
        private int _mapServerIndex = -1;
        private int _mapServerCount = -1;
        private string _serverId = null;
        private string _messageQueueId = null;
        private int _currentPort = 0; // Need better way to do this, list of ints we pick from in concurrent bag?
        private object _currentPortLock = new object();
        private CancellationToken _serverToken;
        public async Task Init(InitMapServerData mapData, CancellationToken serverToken)
        {
            _serverToken = serverToken;
            _currentPort = mapData.StartPort;
            _serverId = ServerNames.MapServer + mapData.MapServerName;
            _mapServerName = mapData.MapServerName;
            _mapServerIndex = mapData.MapServerIndex;
            _mapServerCount = mapData.MapServerCount;

            _messageQueueId = _cloudCommsService.GetFullServerName(_serverId);

            SendAddMapServerMessage();

            List<MapStub> mapStubs = await _mapDataService.GetMapStubs();

            foreach (MapStub stub in mapStubs)
            {
                if (int.TryParse(stub.Id, out int mapStubId))
                {
                    if (MapInstanceConstants.ServerTestMode)
                    {
                        if (mapStubId != 1)
                        {
                            continue;
                        }
                    }
                    if (mapStubId % _mapServerCount == _mapServerIndex)
                    {
                        await CreateMapInstance(stub.Id, serverToken);
                    }
                }
            }
        }

        protected async Task<MapInstance> CreateMapInstance(string mapId, CancellationToken serverToken)
        {
            MapInstance mapInstance = new MapInstance();

            int nextPortNumber = 0;

            lock (_currentPortLock)
            {
                nextPortNumber = ++_currentPort;
            }
            InitMapInstanceData initData = new InitMapInstanceData()
            {
                MapId = mapId,
                Port = nextPortNumber,
                SerializerType = EMapApiSerializers.MessagePack,
            };

            ServerInitArgs args = new ServerInitArgs()
            {
                Token = serverToken,
                Data = initData,
                Parent = mapInstance,
                InitialServices = new List<IInjectable>() { _logService },
            };

            await mapInstance.Init(args);

            _instances[mapInstance.GetInstanceId()] = mapInstance;

            return mapInstance;
        }

        public void SendAddMapServerMessage()
        {
            AddMapServer addServer = new AddMapServer()
            {
                ServerName = _messageQueueId,
            };

            _cloudCommsService.SendQueueMessage(ServerNames.InstanceManager, addServer);

        }

        public IReadOnlyList<MapInstance> GetMapInstances()
        {
            return _instances.Values.ToList();
        }

        private MapInstance GetInstance(string instanceId)
        {
            if (_instances.TryGetValue(instanceId, out MapInstance mapInstance))
            {
                return mapInstance;
            }
            return null;
        }

        public async Task ShutdownInstance(string instanceId)
        {
            MapInstance mapInstance = GetInstance(instanceId);

            if (mapInstance == null)
            {
                return;
            }

            _instances.TryRemove(instanceId, out MapInstance removedInstance);

            await mapInstance.Shutdown();

        }

        public async Task RestartMapsWithId(string mapId)
        {
            IReadOnlyList<MapInstance> restartInstances = GetMapInstances();

            foreach (MapInstance restartInstance in restartInstances)
            {
                if (restartInstance.GetMapId() != mapId)
                {
                    continue;
                }

                await ShutdownInstance(restartInstance.GetInstanceId());

                await CreateMapInstance(restartInstance.GetMapId(), _serverToken);
            }
        }
    }
}


